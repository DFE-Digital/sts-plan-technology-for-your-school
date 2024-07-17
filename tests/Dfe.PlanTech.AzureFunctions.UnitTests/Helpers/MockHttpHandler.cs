namespace Dfe.PlanTech.AzureFunctions.UnitTests.Helpers;

public class MockHttpHandler : HttpMessageHandler
{
    public readonly List<HttpRequestMessage> Requests = [];
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return await Task.FromResult(new HttpResponseMessage());
    }
}
