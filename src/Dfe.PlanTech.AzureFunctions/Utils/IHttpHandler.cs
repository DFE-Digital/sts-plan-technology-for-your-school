namespace Dfe.PlanTech.AzureFunctions.Utils;

public interface IHttpHandler
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
}
