using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions
{
  public class TestingJsonParser
  {
    private readonly ILogger _logger;

    private readonly CmsDbContext _db;

    private readonly Mappers _mappers;

    public TestingJsonParser(ILoggerFactory loggerFactory, CmsDbContext db, Mappers mappers)
    {
      _logger = loggerFactory.CreateLogger<TestingJsonParser>();
      _db = db;

      _mappers = mappers;
    }

    [Function("TestingSerialisation")]
    public async Task<HttpResponseData> TestingSerialisation([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
      var stream = new StreamReader(req.Body);
      var body = stream.ReadToEnd();

      try
      {
        var answerDbEntity = _mappers.ToEntity(body);

        return req.CreateResponse(HttpStatusCode.OK);
      }
      catch (Exception ex)
      {
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);

        var sb = new StringBuilder();

        var currentException = ex;

        while (currentException != null)
        {
          sb.AppendLine(currentException.Message);
          sb.AppendLine(currentException.StackTrace);
          currentException = ex.InnerException;
        }
        await response.WriteStringAsync(sb.ToString());

        return response;
      }
    }

    [Function("TestingJsonParser")]
    public async Task<HttpResponseData> WebhookReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
      var stream = new StreamReader(req.Body);
      var body = stream.ReadToEnd();

      try
      {
        var processed = TryNormaliseJson(body);

        var responseType = processed ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

        var response = req.CreateResponse(responseType);

        return response;
      }
      catch (Exception ex)
      {
        var response = req.CreateResponse(HttpStatusCode.InternalServerError);

        var sb = new StringBuilder();

        var currentException = ex;

        while (currentException != null)
        {
          sb.AppendLine(currentException.Message);
          sb.AppendLine(currentException.StackTrace);
          currentException = ex.InnerException;
        }
        await response.WriteStringAsync(sb.ToString());

        return response;
      }
    }

    private bool TryNormaliseJson(string body)
    {
      var mapped = _mappers.ToEntity(body);

      if (mapped == null)
      {
        _logger.LogError($"Received null object back");
        return false;
      }

      if (mapped is not ContentComponentDbEntity contentComponent)
      {
        return false;
      }

      var existing = _db.Find(mapped.GetType(), contentComponent.Id);

      if (existing != null)
      {
        var properties = mapped.GetType().GetProperties();

        foreach (var property in properties)
        {
          property.SetValue(existing, property.GetValue(mapped));
        }
      }
      else
      {
        _db.Add(mapped);
      }

      _db.SaveChanges();

      return true;
    }
  }
}
