using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class RecommendationRepository(PlanTechDbContext dbContext) : IRecommendationRepository
{
    protected readonly PlanTechDbContext _db =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<
        IEnumerable<RecommendationEntity>
    > GetRecommendationsByContentfulReferencesAsync(
        IEnumerable<string> recommendationContentfulReferences
    )
    {
        return await _db
            .Recommendations.Where(r =>
                recommendationContentfulReferences.Contains(r.ContentfulRef)
            )
            .ToListAsync();
    }

    public async Task<List<RecommendationEntity>> UpsertRecommendations(
        IEnumerable<SqlRecommendationDto> recommendationDtos
    )
    {
        var contentfulRefs = recommendationDtos.Select(r => r.ContentfulSysId);
        var existingRecommendations = await _db
            .Recommendations.Where(recommendation =>
                contentfulRefs.Contains(recommendation.ContentfulRef)
            )
            .Where(recommendation => recommendation != null)
            .GroupBy(recommendation => recommendation.ContentfulRef)
            .Select(group => group.OrderByDescending(g => g.DateCreated).First())
            .ToListAsync();

        var existingRecommendationContentfulRefs = existingRecommendations
            .Select(r => r.ContentfulRef)
            .ToList();

        var recommendationEntitiesToInsert = recommendationDtos
            .Where(rm => !existingRecommendationContentfulRefs.Contains(rm.ContentfulSysId))
            .Select(RecommendationEntity.BuildEntity)
            .ToList();

        var recommendationDtoDictionary = recommendationDtos.ToDictionary(
            r => r.ContentfulSysId,
            r => r
        );
        var recommendationsWithNoChanges = new List<RecommendationEntity>();

        foreach (var existingRecommendation in existingRecommendations)
        {
            recommendationDtoDictionary.TryGetValue(
                existingRecommendation.ContentfulRef,
                out var recommendationDto
            );
            if (recommendationDto is null)
            {
                continue;
            }

            var recommendationEntity = RecommendationEntity.BuildEntity(recommendationDto);
            if (
                !string.Equals(
                    recommendationDto.RecommendationText,
                    existingRecommendation.RecommendationText
                )
            )
            {
                recommendationEntitiesToInsert.Add(recommendationEntity);
            }
            else
            {
                recommendationsWithNoChanges.Add(recommendationEntity);
            }
        }

        _db.AddRange(recommendationEntitiesToInsert);
        await _db.SaveChangesAsync();

        return await _db
            .Recommendations.Where(r => contentfulRefs.Contains(r.ContentfulRef))
            .ToListAsync();
    }
}
