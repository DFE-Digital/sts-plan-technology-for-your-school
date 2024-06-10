using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class QuestionMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<JsonToDbMapper<QuestionDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<QuestionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private readonly List<AnswerDbEntity> _incomingAnswers = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        UpdateAnswersParentQuestionIds(values);

        return values;
    }

    private void UpdateAnswersParentQuestionIds(Dictionary<string, object?> values)
    {
        int order = 0;

        foreach (var answer in GetReferences<AnswerDbEntity>(values, "answers"))
        {
            answer.Order = order++;
            _incomingAnswers.Add(answer);
        }
    }

    public override async Task<MappedEntity> MapEntity(CmsWebHookPayload payload, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var incomingEntity = ToEntity(payload);

        var existingEntity = await _db.Questions.Include(q => q.Answers)
                                                .FirstOrDefaultAsync(question => question.Id == incomingEntity.Id, cancellationToken: cancellationToken);

        var mappedEntity = _entityUpdater.UpdateEntity(incomingEntity, existingEntity, cmsEvent);

        if (!mappedEntity.AlreadyExistsInDatabase)
        {
            return mappedEntity;
        }

        var withoutOldQuestions = RemoveQuestionFromRemovedAnswers(mappedEntity);
        await AddOrUpdateQuestions(existingEntity);

        return mappedEntity;
    }

    private async Task AddOrUpdateQuestions(QuestionDbEntity existingEntity)
    {
        foreach (var incomingAnswer in _incomingAnswers)
        {
            var matchingAnswer = existingEntity.Answers.FirstOrDefault(answer => answer.Id == incomingAnswer.Id);
            if (matchingAnswer == null)
            {
                var dbAnswer = await _db.Answers.FirstOrDefaultAsync(answer => answer.Id == incomingAnswer.Id);
                if(dbAnswer == null){
                    Logger.LogError($"Question {existingEntity.Id} is trying to add answer {incomingAnswer.Id} but this is not found in the DB");
                    continue;
                }

                existingEntity.Answers.Add(dbAnswer);
                dbAnswer.ParentQuestionId = existingEntity.Id;
            }
            else
            {
                matchingAnswer.Order = incomingAnswer.Order;
            }
        }
    }

    protected MappedEntity RemoveQuestionFromRemovedAnswers(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<QuestionDbEntity>();

        var answersToRemove = existing.Answers.Where(existingAnswer => !_incomingAnswers.Exists(incomingAnswer => incomingAnswer.Id == existingAnswer.Id))
                                                .ToArray();

        foreach (var answerToRemove in answersToRemove)
        {
            existing.Answers.Remove(answerToRemove);
            answerToRemove.ParentQuestionId = null;
        }

        return mappedEntity;
    }
}
