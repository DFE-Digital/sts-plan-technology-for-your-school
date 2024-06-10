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
        int order = 0;
        values = MoveValueToNewKey(values, "interstitialPage", "interstitialPageId");

        foreach (var question in GetReferences<QuestionDbEntity>(values, "questions"))
        {
            question.Order = order++;
            _incomingQuestions.Add(question);
        }

        return values;
    }


    public override async Task<MappedEntity> MapEntity(CmsWebHookPayload payload, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var incomingEntity = ToEntity(payload);

        var existingEntity = await _db.Sections.Include(q => q.Questions)
                                                .FirstOrDefaultAsync(section => section.Id == incomingEntity.Id, cancellationToken: cancellationToken);

        var mappedEntity = _entityUpdater.UpdateEntity(incomingEntity, existingEntity, cmsEvent);

        if (!mappedEntity.AlreadyExistsInDatabase)
        {
            return mappedEntity;
        }

        RemoveSectionFromRemovedQuestions(mappedEntity);


        if (mappedEntity.ExistingEntity is not SectionDbEntity existingSection)
        {
            Logger.LogError("Section is not a section. Is type " + mappedEntity.ExistingEntity?.GetType());
            return mappedEntity;
        }


        await AddOrUpdateQuestions(existingSection);

        return mappedEntity;
    }

    private async Task AddOrUpdateQuestions(SectionDbEntity existingEntity)
    {
        foreach (var incomingQuestion in _incomingQuestions)
        {
            var matchingQuestion = existingEntity.Questions.FirstOrDefault(question => question.Id == incomingQuestion.Id);
            if (matchingQuestion == null)
            {
                var dbQuestion = await _db.Questions.FirstOrDefaultAsync(question => question.Id == incomingQuestion.Id);
                if(dbQuestion == null){
                    Logger.LogError($"Section {existingEntity.Id} is trying to add question {incomingQuestion.Id} but this is not found in the DB");
                    continue;
                }

                existingEntity.Questions.Add(dbQuestion);
                dbQuestion.SectionId = existingEntity.Id;
            }
            else
            {
                matchingQuestion.Order = incomingQuestion.Order;
            }
        }
    }

    protected MappedEntity RemoveSectionFromRemovedQuestions(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<SectionDbEntity>();

        var questionsToRemove = existing.Questions.Where(existingQuestion => !_incomingQuestions.Exists(incomingQuestion => incomingQuestion.Id == existingQuestion.Id))
                                                    .ToArray();

        foreach (var questionToRemove in questionsToRemove)
        {
            existing.Questions.Remove(questionToRemove);
            questionToRemove.SectionId = null;
        }

        return mappedEntity;
    }
}