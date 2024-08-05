using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<RecommendationChunkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<RecommendationChunkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;

    private List<AnswerDbEntity> _incomingAnswers = [];
    private List<ContentComponentDbEntity> _incomingContent = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");
        values = MoveValueToNewKey(values, "csLink", "csLinkId");

        _incomingAnswers = _entityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers").ToList();
        _incomingContent = _entityUpdater.GetAndOrderReferencedEntities<ContentComponentDbEntity>(values, "content").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationChunkDbEntity>();

        if (existing != null)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            if (existing.Answers == null || existing.Answers.Count == 0)
            {
                await _db.RecommendationChunkAnswers.IgnoreQueryFilters().Where(recChunkAnswer => recChunkAnswer.RecommendationChunkId == existing.Id).Include(rca => rca.Answer).ToListAsync();
            }

            if (existing.Content == null || existing.Content.Count == 0)
            {
                await _db.RecommendationChunkContents.IgnoreQueryFilters().Where(recChunkContent => recChunkContent.RecommendationChunkId == existing.Id).Include(rca => rca.ContentComponent).ToListAsync();
            }
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Answers, _incomingAnswers, _db.Answers, false);
        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Content, _incomingContent, _db.ContentComponents, true);
    }
}
