namespace Dfe.PlanTech.AzureFunctions.Utils;

public class HttpHandler : IHttpHandler
{
    private readonly HttpClient _httpClient = new();

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) =>
        await _httpClient.SendAsync(request);
}
