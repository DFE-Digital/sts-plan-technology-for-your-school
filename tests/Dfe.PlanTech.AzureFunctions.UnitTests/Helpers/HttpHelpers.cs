using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public static class HttpHelpers
{
    public static HttpRequestData MockHttpRequest()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var context = Substitute.For<FunctionContext>();
        context.InstanceServices.Returns(serviceProvider);

        var request = Substitute.For<HttpRequestData>(context);

        request.CreateResponse().Returns((callInfo) =>
        {
            var response = Substitute.For<HttpResponseData>(context);
            response.Headers.Returns(new HttpHeadersCollection());
            response.Body.Returns(new MemoryStream());

            return response;
        });

        return request;
    }
}
