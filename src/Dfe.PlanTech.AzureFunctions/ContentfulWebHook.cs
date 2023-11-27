using System.Net;
using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.AzureFunctions.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
    public class ContentfulWebHook
    {
        private readonly ILogger _logger;
        private readonly ServiceBusClient _client;

        public ContentfulWebHook(ILoggerFactory loggerFactory, ServiceBusClient serviceBusClient)
        {
            _logger = loggerFactory.CreateLogger<ContentfulWebHook>();
            _client = serviceBusClient;
        }

        [Function("ContentfulWebHook")]
        public async Task<HttpResponseData> WebhookReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var stream = new StreamReader(req.Body);
            var body = stream.ReadToEnd();

            try
            {

                //todo - use DI
                await using var sender = new QueueSender(_client, "contentful");

                await sender.SendMessage(body);

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
