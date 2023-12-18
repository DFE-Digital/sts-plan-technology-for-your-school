using System.Net;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
    public class ContentfulWebHook : BaseFunction
    {
        private readonly ServiceBusSender _sender;

        public ContentfulWebHook(ILoggerFactory loggerFactory, IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory) : base(loggerFactory.CreateLogger<ContentfulWebHook>())
        {
            _sender = serviceBusSenderFactory.CreateClient("contentful");
        }

        [Function("ContentfulWebHook")]
        public async Task<HttpResponseData> WebhookReceiver([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            Logger.LogInformation("Received webhook POST.");

            var stream = new StreamReader(req.Body);
            var body = stream.ReadToEnd();

            if (string.IsNullOrEmpty(body))
            {
                return ReturnEmptyBodyError(req);
            }

            Logger.LogTrace("Logging message body: {body}", body);

            try
            {
                await WriteToQueue(body);

                return ReturnOkResponse(req);
            }
            catch (Exception ex)
            {
                return ReturnServerErrorResponse(req, ex);
            }
        }

        protected Task WriteToQueue(string body)
        {
            var serviceBusMessage = new ServiceBusMessage(body);
            return _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
