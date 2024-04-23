using System.Text.Json.Nodes;
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
    var databaseSubtopicRecommendation = ValidateChildEntityExistsInDb(databaseSubtopicRecommendations, contentfulSubtopicRecommendation);
    if (databaseSubtopicRecommendation == null)
    {
      return;
    }

    var sectionValidationResult = ValidateChild<SubtopicRecommendationDbEntity>(databaseSubtopicRecommendation, "SectionId", contentfulSubtopicRecommendation, "section");
    var subtopicValidationResult = ValidateChild<SubtopicRecommendationDbEntity>(databaseSubtopicRecommendation, "SubtopicId", contentfulSubtopicRecommendation, "subtopic");

    ValidateProperties(contentfulSubtopicRecommendation, databaseSubtopicRecommendation, sectionValidationResult, subtopicValidationResult);
    ValidateChildren(contentfulSubtopicRecommendation, "intros", databaseSubtopicRecommendation, dbSubtopicRecommendation => dbSubtopicRecommendation.Intros);
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