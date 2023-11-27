using Azure.Identity;

namespace Dfe.PlanTech.AzureFunctions.Auth;

public static class IdentityAuthentication
{
  public static DefaultAzureCredential GetAzureCredentials()
  {
    return new DefaultAzureCredential(includeInteractiveCredentials: true);
  }
}