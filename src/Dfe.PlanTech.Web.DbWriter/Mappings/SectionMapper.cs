using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class SectionMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<SectionMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<SectionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private List<QuestionDbEntity> _incomingQuestions = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "interstitialPage", "interstitialPageId");

        _incomingQuestions = _entityUpdater.GetAndOrderReferencedEntities<QuestionDbEntity>(values, "questions").ToList();

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
