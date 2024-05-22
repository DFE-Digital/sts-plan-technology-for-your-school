using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SectionMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<SectionMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<SectionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        int order = 0;
        values = MoveValueToNewKey(values, "interstitialPage", "interstitialPageId");

        UpdateReferencesArray(values, "questions", _db.Questions, (id, question) =>
        {
            question.SectionId = Payload!.Sys.Id;
            question.Order = order++;
        });

        return values;
    }

}