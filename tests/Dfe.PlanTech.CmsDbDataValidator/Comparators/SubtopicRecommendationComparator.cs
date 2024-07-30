using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class SubtopicRecommendationsComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, [], "SubtopicRecommendation")
{
    public override Task ValidateContent()
    {
        ValidateSubtopicRecommendations();
        return Task.CompletedTask;
    }

    public void ValidateSubtopicRecommendations()
    {
        foreach (var contentfulSubtopicRecommendation in _contentfulEntities)
        {
            ValidateSubtopicRecommendation(_dbEntities.OfType<SubtopicRecommendationDbEntity>().ToArray(), contentfulSubtopicRecommendation);
        }
    }

    private void ValidateSubtopicRecommendation(SubtopicRecommendationDbEntity[] databaseSubtopicRecommendations, JsonNode contentfulSubtopicRecommendation)
    {
        var databaseSubtopicRecommendation = TryRetrieveMatchingDbEntity(databaseSubtopicRecommendations, contentfulSubtopicRecommendation);
        if (databaseSubtopicRecommendation == null)
        {
            return;
        }

        ValidateProperties(contentfulSubtopicRecommendation, databaseSubtopicRecommendation, GetValidationErrors(databaseSubtopicRecommendation, contentfulSubtopicRecommendation).ToArray());
    }


    protected IEnumerable<DataValidationError> GetValidationErrors(SubtopicRecommendationDbEntity databaseSubtopicRecommendation, JsonNode contentfulSubtopicRecommendation)
    {
        var sectionValidationResult = ValidateChild<SubtopicRecommendationDbEntity>(databaseSubtopicRecommendation, "SectionId", contentfulSubtopicRecommendation, "section");

        if (sectionValidationResult != null)
        {
            yield return new DataValidationError("SectionId", sectionValidationResult);
        }

        var subtopicValidationResult = ValidateChild<SubtopicRecommendationDbEntity>(databaseSubtopicRecommendation, "SubtopicId", contentfulSubtopicRecommendation, "subtopic");

        if (subtopicValidationResult != null)
        {
            yield return new DataValidationError("SubtopicId", subtopicValidationResult);
        }

        foreach (var child in ValidateChildren(contentfulSubtopicRecommendation, "intros", databaseSubtopicRecommendation, dbRecommendationChunk => dbRecommendationChunk.Intros))
        {
            yield return child;
        }
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.SubtopicRecommendations.IgnoreQueryFilters().Select(subtopicRecommendation => new SubtopicRecommendationDbEntity()
        {
            Id = subtopicRecommendation.Id,
            SectionId = subtopicRecommendation.SectionId,
            SubtopicId = subtopicRecommendation.SubtopicId,
            Intros = subtopicRecommendation.Intros.Select(intro => new RecommendationIntroDbEntity()
            {
                Id = intro.Id
            }).ToList()
        });
    }
}
