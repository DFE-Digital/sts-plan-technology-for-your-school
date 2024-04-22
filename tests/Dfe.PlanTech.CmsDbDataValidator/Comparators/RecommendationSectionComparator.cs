using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public class RecommendationSectionsComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, [], "RecommendationSection")
{
  public override Task ValidateContent()
  {
    ValidateRecommendationSections();
    return Task.CompletedTask;
  }

  public void ValidateRecommendationSections()
  {
    foreach (var contentfulRecommendationSection in _contentfulEntities)
    {
      ValidateRecommendationSection(_dbEntities.OfType<RecommendationSectionDbEntity>().ToArray(), contentfulRecommendationSection);
    }
  }

  private void ValidateRecommendationSection(RecommendationSectionDbEntity[] databaseRecommendationSections, JsonNode contentfulRecommendationSection)
  {
    var databaseRecommendationSection = ValidateChildEntityExistsInDb(databaseRecommendationSections, contentfulRecommendationSection);
    if (databaseRecommendationSection == null)
    {
      return;
    }

    ValidateChildren(contentfulRecommendationSection, "answers", databaseRecommendationSection, dbRecommendationSection => dbRecommendationSection.Answers);
    ValidateChildren(contentfulRecommendationSection, "chunks", databaseRecommendationSection, dbRecommendationSection => dbRecommendationSection.Chunks);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.RecommendationSections.Select(section => new RecommendationSectionDbEntity()
    {
      Id = section.Id,
      Answers = section.Answers.Select(answer => new AnswerDbEntity()
      {
        Id = answer.Id
      }).ToList(),
      Chunks = section.Chunks.Select(chunk => new RecommendationChunkDbEntity()
      {
        Id = chunk.Id
      }).ToList()
    });
  }
}