namespace Dfe.PlanTech.AzureFunctions.Utils;

public class HttpHandler: IHttpHandler
{
    private readonly HttpClient _httpClient = new ();

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage) =>
        await _httpClient.SendAsync(requestMessage);
}
