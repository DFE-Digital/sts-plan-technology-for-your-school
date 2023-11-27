using System.Net;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
    public class ContentfulWebHook
    {
        private readonly ILogger _logger;
        private readonly ServiceBusSender _sender;

        public ContentfulWebHook(ILoggerFactory loggerFactory, IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory)
        {
            _logger = loggerFactory.CreateLogger<ContentfulWebHook>();
            _sender = serviceBusSenderFactory.CreateClient("contentful");
        }

        [Function("ContentfulWebHook")]
        public async Task<HttpResponseData> WebhookReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var stream = new StreamReader(req.Body);
            var body = stream.ReadToEnd();

            try
            {
                var serviceBusMessage = new ServiceBusMessage(body);

                await _sender.SendMessageAsync(serviceBusMessage);

                var response = req.CreateResponse(HttpStatusCode.OK);

                return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(ex.Message);

                return response;
            }
        }
    }
}
