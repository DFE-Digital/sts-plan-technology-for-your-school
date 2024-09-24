using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class QuestionMapper(EntityUpdater updater,
                            ILogger<JsonToDbMapper<QuestionDbEntity>> logger,
                            JsonSerializerOptions jsonSerialiserOptions,
                            IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<QuestionDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<AnswerDbEntity> _incomingAnswers = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        _incomingAnswers = _entityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<QuestionDbEntity>();

        if (existing != null)
        {
            existing.Answers = await GetEntitiesMatchingPredicate<AnswerDbEntity>(answer => answer.ParentQuestionId == existing.Id, cancellationToken);
            Logger.LogTrace("Retrieved answers for existing question ID \"{QuestionId}\": \"{Answers}\"", existing.Id, string.Join(",", existing.Answers.Select(answer => answer.Id)));
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (question) => question.Answers, _incomingAnswers, true, cancellationToken);
    }
}
