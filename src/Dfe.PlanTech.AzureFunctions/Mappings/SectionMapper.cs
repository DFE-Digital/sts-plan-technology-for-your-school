using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SectionMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<SectionMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<SectionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private readonly List<string> _incomingQuestionIds = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        int order = 0;
        values = MoveValueToNewKey(values, "interstitialPage", "interstitialPageId");

        UpdateReferencesArray(values, "questions", _db.Questions, (id, question) =>
        {
            question.SectionId = Payload!.Sys.Id;
            question.Order = order++;
            _incomingQuestionIds.Add(question.Id);
        });

        return values;
    }


    public override async Task<MappedEntity> MapEntity(CmsWebHookPayload payload, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var mappedEntity = await base.MapEntity(payload, cmsEvent, cancellationToken);

        if (!mappedEntity.AlreadyExistsInDatabase)
        {
            return mappedEntity;
        }

        return RemoveSectionFromRemovedQuestions(mappedEntity);
    }

    protected MappedEntity RemoveSectionFromRemovedQuestions(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<SectionDbEntity>();

        var questionsToRemove = existing.Questions.Where(existingQuestion => !_incomingQuestionIds.Exists(incomingQuestion => incomingQuestion == existingQuestion.Id))
                                                    .ToArray();

        foreach (var questionToRemove in questionsToRemove)
        {
            existing.Questions.Remove(questionToRemove);
            questionToRemove.SectionId = null;
        }

        return mappedEntity;
    }
}