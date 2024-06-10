using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SectionMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<SectionMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<SectionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private readonly List<QuestionDbEntity> _incomingQuestions = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "interstitialPage", "interstitialPageId");
        _incomingQuestions.AddRange(_entityUpdater.GetAndOrderReferencedEntities<QuestionDbEntity>(values, "questions"));

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<SectionDbEntity>();

        if (existing != null)
        {
            existing.Questions = await _db.Questions.Where(question => question.SectionId == incoming.Id).ToListAsync();
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (section) => section.Questions, _incomingQuestions, _db.Questions, true);
    }
}