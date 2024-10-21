using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Authorisation;

public class SigningSecretAuthorisationFilter(
    [FromServices] ISystemTime systemTime,
    SigningSecretConfiguration signingSecretConfiguration,
    ILogger<SigningSecretAuthorisationFilter> logger
) : IAuthorizationFilter
{
    public const string HeaderSignature = "x-contentful-signature";
    public const string HeaderTimestamp = "x-contentful-timestamp";
    public const string HeaderSignedValues = "x-contentful-signed-headers";
    public const int RequestTimeToLiveMinutes = 5;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var signingSecret = signingSecretConfiguration.SigningSecret;
        if (signingSecret.IsNullOrEmpty())
        {
            logger.LogError("Signing secret config value missing");
            context.Result = new UnauthorizedResult();
        }

        if (VerifyRequest(signingSecret, context.HttpContext.Request))
            return;

        logger.LogError("Request verification with signing secret failed");
        context.Result = new UnauthorizedResult();
    }

    /// <summary>
    /// Forms a canonical representation of the request, uses the signing secret to generate a HMAC_SHA256 hash
    /// and then compares the generated signature with the one in the request.
    /// </summary>
    private bool VerifyRequest(string signingKey, HttpRequest request)
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
        if (timestamp.AddMinutes(RequestTimeToLiveMinutes) <= systemTime.UtcNow)
        {
            logger.LogError("Request to CMS route denied due to expired timestamp");
            return false;
        }

        var canonicalRepresentation = CreateCanonicalRepresentation(request, requestSignedHeaders);
        var signature = CreateSignature(canonicalRepresentation, signingKey);

        return signature == requestSignature;
    }

    public static string CreateCanonicalRepresentation(HttpRequest request, StringValues signedHeaders)
    {
        var requestPath = request.GetEncodedPathAndQuery();
        var requestHeaders = string.Join(";", signedHeaders
            .ToString()
            .Split(',')
            .Select(header => header.ToLower() + ":" + request.Headers[header]));
        var requestBody = new StreamReader(request.Body).ReadToEndAsync().Result;

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
