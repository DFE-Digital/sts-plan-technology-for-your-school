using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class QuestionMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<JsonToDbMapper<QuestionDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<QuestionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private readonly List<string> _incomingAnswerIds = [];
    
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        UpdateAnswersParentQuestionIds(values);

        return values;
    }

    private void UpdateAnswersParentQuestionIds(Dictionary<string, object?> values)
    {
        int order = 0;

        UpdateReferencesArray(values, "answers", _db.Answers, (id, answer) =>
        {
            answer.ParentQuestionId = Payload!.Sys.Id;
            answer.Order = order++;

            _incomingAnswerIds.Add(answer.Id);
        });
    }

    public override async Task<MappedEntity> MapEntity(CmsWebHookPayload payload, CmsEvent cmsEvent, CancellationToken cancellationToken)
    {
        var mappedEntity = await base.MapEntity(payload, cmsEvent, cancellationToken);

        if (!mappedEntity.AlreadyExistsInDatabase)
        {
            return mappedEntity;
        }

        return RemoveQuestionFromRemovedAnswers(mappedEntity);
    }

    protected MappedEntity RemoveQuestionFromRemovedAnswers(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<QuestionDbEntity>();

        var answersToRemove = existing.Answers.Where(existingAnswer => !_incomingAnswerIds.Exists(incomingAnswer => incomingAnswer == existingAnswer.Id))
                                                .ToArray();

        foreach (var answerToRemove in answersToRemove)
        {
            existing.Answers.Remove(answerToRemove);
            answerToRemove.ParentQuestionId = null;
        }

        return mappedEntity;
    }
}
