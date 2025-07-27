using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Web.Authorisation.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Authorisation.Policies;

public class SignedRequestAuthorisationPolicy(
    SigningSecretConfiguration signingSecretConfiguration,
    ILogger<SignedRequestAuthorisationPolicy> logger
) : AuthorizationHandler<SignedRequestAuthorisationRequirement>
{
    public const string PolicyName = "UseSignedRequestAuthentication";

    public const string HeaderSignature = "x-contentful-signature";
    public const string HeaderTimestamp = "x-contentful-timestamp";
    public const string HeaderSignedValues = "x-contentful-signed-headers";
    public const int RequestTimeToLiveMinutes = 5;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SignedRequestAuthorisationRequirement requirement)
    {
        var signingSecret = signingSecretConfiguration.SigningSecret;
        if (signingSecret.IsNullOrEmpty())
        {
            logger.LogError("Signing secret config value missing");
            context.Fail();
            return;
        }

        if (context.Resource is HttpContext httpContext && await VerifyRequest(signingSecret, httpContext.Request))
        {
            context.Succeed(requirement);
            return;
        }

        logger.LogError("Request verification with signing secret failed");
        context.Fail();
    }

    /// <summary>
    /// Forms a canonical representation of the request, uses the signing secret to generate a HMAC_SHA256 hash
    /// and then compares the generated signature with the one in the request.
    /// </summary>
    private async Task<bool> VerifyRequest(string signingKey, HttpRequest request)
    {
        if (!request.Headers.TryGetValue(HeaderSignature, out var requestSignature) ||
            !request.Headers.TryGetValue(HeaderTimestamp, out var requestTimestamp) ||
            !request.Headers.TryGetValue(HeaderSignedValues, out var requestSignedHeaders) ||
            string.IsNullOrEmpty(requestSignature) ||
            string.IsNullOrEmpty(requestTimestamp) ||
            string.IsNullOrEmpty(requestSignedHeaders))
        {
            logger.LogError("Request to CMS route denied due to missing headers");
            return false;
        }

        // Check timestamp is within the TTL
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(requestTimestamp!, CultureInfo.InvariantCulture));
        if (timestamp.AddMinutes(RequestTimeToLiveMinutes) <= DateTime.UtcNow)
        {
            logger.LogError("Request to CMS route denied due to expired timestamp");
            return false;
        }

        var canonicalRepresentation = await CreateCanonicalRepresentation(request, requestSignedHeaders);
        var signature = CreateSignature(canonicalRepresentation, signingKey);

        return signature == requestSignature;
    }

    public static async Task<string> CreateCanonicalRepresentation(HttpRequest request, StringValues signedHeaders)
    {
        var requestPath = request.GetEncodedPathAndQuery();
        var requestHeaders = string.Join(";", signedHeaders
            .ToString()
            .Split(',')
            .Select(header => header.ToLower() + ":" + request.Headers[header]));

        request.EnableBuffering();
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;

        return string.Join("\n", request.Method, requestPath, requestHeaders, requestBody);
    }

    private static string CreateSignature(string message, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmacsha256 = new HMACSHA256(keyBytes);
        var hashBytes = hmacsha256.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
