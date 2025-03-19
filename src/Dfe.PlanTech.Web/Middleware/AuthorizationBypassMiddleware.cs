using System.Security.Claims;
using Dfe.PlanTech.Domain.SignIns.Enums;

namespace Dfe.PlanTech.Web.Middleware
{
    public class AuthorizationBypassMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public AuthorizationBypassMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            //Set by an environment variable/keyvault
            bool bypassAuthorization = true;

            if (bypassAuthorization)
            {
                var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, "BypassedUser"), new Claim( ClaimConstants.Organisation, GetOrganisationJson()),
                    new Claim(ClaimConstants.DB_USER_ID, "25"),
                    new Claim(ClaimConstants.DB_ESTABLISHMENT_ID, "101")
                }, "BypassAuth"));
                context.User = user;
            }

            await _next(context);
        }
        private string GetOrganisationJson()
        {
            return """
                   {
                     "id": "4EBBE3F9-F0C0-4833-9B60-4A04BD29AE0E",
                     "name": "DSI TEST Establishment (001) Community School (01)",
                     "LegalName": null,
                     "category": {
                       "id": "001",
                       "name": "Establishment"
                     },
                     "type": {
                       "id": "01",
                       "name": "Community School"
                     },
                     "urn": null,
                     "uid": null,
                     "upin": null,
                     "ukprn": "00000002",
                     "establishmentNumber": null,
                     "status": {
                       "id": 1,
                       "name": "Open",
                       "tagColor": "green"
                     },
                     "closedOn": null,
                     "address": null,
                     "telephone": null,
                     "statutoryLowAge": null,
                     "statutoryHighAge": null,
                     "legacyId": "4008569",
                     "companyRegistrationNumber": null,
                     "SourceSystem": null,
                     "providerTypeName": null,
                     "ProviderTypeCode": null,
                     "GIASProviderType": null,
                     "PIMSProviderType": null,
                     "PIMSProviderTypeCode": null,
                     "PIMSStatusName": null,
                     "pimsStatus": null,
                     "GIASStatusName": null,
                     "GIASStatus": null,
                     "MasterProviderStatusName": null,
                     "MasterProviderStatusCode": null,
                     "OpenedOn": null,
                     "DistrictAdministrativeName": null,
                     "DistrictAdministrativeCode": null,
                     "DistrictAdministrative_code": null,
                     "IsOnAPAR": null
                   }
                   """;
        }
    }
}
