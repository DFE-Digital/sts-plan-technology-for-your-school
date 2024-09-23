using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class QuestionMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<JsonToDbMapper<QuestionDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<QuestionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private List<AnswerDbEntity> _incomingAnswers = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        _incomingAnswers = _entityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<QuestionDbEntity>();

        if (existing != null)
        {
            existing.Answers = await _db.Answers
                                        .Where(answer => answer.ParentQuestionId == incoming.Id)
                                        .Select(answer => answer)
                                        .ToListAsync();

            Logger.LogTrace("Retrieved answers for existing question ID \"{QuestionId}\": \"{Answers}\"", existing.Id, string.Join(",", existing.Answers.Select(answer => answer.Id)));
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (question) => question.Answers, _incomingAnswers, _db.Answers, true);
    }
}
