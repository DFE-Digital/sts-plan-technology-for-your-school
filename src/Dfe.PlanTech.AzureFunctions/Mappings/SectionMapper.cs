using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SectionMapper : JsonToDbMapper<SectionDbEntity>
{
    private readonly CmsDbContext _db;

    public SectionMapper(CmsDbContext db, ILogger<SectionMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
        _db = db;
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        int order = 0;
        values = MoveValueToNewKey(values, "interstitialPage", "interstitialPageId");

        UpdateReferencesArray(values, "questions", _db.Questions, (id, question) =>
        {
            question.SectionId = Payload!.Sys.Id;
            question.Order = order++;
        });

        UpdateReferencesArray(values, "recommendations", _db.RecommendationPages, (id, recommendationPage) => recommendationPage.SectionId = Payload!.Sys.Id);

        return values;
    }

}