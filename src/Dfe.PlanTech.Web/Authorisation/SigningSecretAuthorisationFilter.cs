using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Dfe.PlanTech.Web.Authorisation;

public class SigningSecretAuthorisationFilter(SigningSecretConfiguration signingSecretConfiguration, ILogger<ApiKeyAuthorisationFilter> logger) : IAuthorizationFilter
{
    private const string HeaderSignature = "x-contentful-signature";
    private const string HeaderTimestap = "x-contentful-timestap";
    private const string HeaderSignedValues = "x-contentful-signed-headers";
    private const int RequestTimeToLiveMinutes = 5;

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
            !request.Headers.TryGetValue(HeaderTimestap, out var requestTimestamp) ||
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
        if (timestamp.AddMinutes(RequestTimeToLiveMinutes) <= SystemTime.UtcNow)
        {
            logger.LogError("Request to CMS route denied due to expired timestamp");
            return false;
        }

        // Get utf-8 encoded request path excluding protocol, hostname and port
        var requestPath = request.GetEncodedPathAndQuery();

        // Headers should be converted to lower case in a semi-colon separated list
        var requestHeaders = string.Join(";", requestSignedHeaders
            .ToString()
            .Split(',')
            .Select(header => header.ToLower() + ":" + request.Headers[header]));

        var canonicalRepresentation = string.Join("\n", request.Method, requestPath, requestHeaders, request.Body);
        var signature = CreateSignature(canonicalRepresentation, signingKey);

        // Compare request signature to generated signature
        return signature == requestSignature;
    }

    private static string CreateSignature(string message, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using (var hmacsha256 = new HMACSHA256(keyBytes))
        {
            var hashBytes = hmacsha256.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
