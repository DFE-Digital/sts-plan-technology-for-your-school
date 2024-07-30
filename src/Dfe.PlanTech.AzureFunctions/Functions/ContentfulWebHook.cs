using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Domain.Caching.Exceptions;
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
            var body = await stream.ReadToEndAsync(); // Await ReadToEndAsync instead
            var cmsEvent = req.Headers.GetValues("X-Contentful-Topic").FirstOrDefault();

            if (string.IsNullOrEmpty(body))
            {
                return ReturnEmptyBodyError(req);
            }

            Logger.LogTrace("Logging message body: {body}", body);

            try
            {
                if (string.IsNullOrEmpty(cmsEvent))
                {
                    throw new CmsEventException("CMS Event is NULL or Empty");
                }

                await WriteToQueue(body, cmsEvent);

                return ReturnOkResponse(req);
            }
            catch (Exception ex)
            {
                return ReturnServerErrorResponse(req, ex);
            }
        }

        protected Task WriteToQueue(string body, string cmsEvent)
        {
            var serviceBusMessage = new ServiceBusMessage(body) { Subject = cmsEvent };
            return _sender.SendMessageAsync(serviceBusMessage);
        }
    }
}
