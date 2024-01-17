using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

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

        UpdateInterstitialPage(values);

        return values;
    }

    private void UpdateInterstitialPage(Dictionary<string, object?> values)
    {
        if (values.TryGetValue("interstitialPageId", out var pageId) && pageId != null)
        {
            var interstitialPageId = pageId.ToString();

            if (!string.IsNullOrWhiteSpace(interstitialPageId))
            { 
                UpdateRelatedEntity(interstitialPageId, _db.Pages, (id, interstitialPage) => interstitialPage.SectionId = Payload!.Sys.Id);
            }
        }
    }
}