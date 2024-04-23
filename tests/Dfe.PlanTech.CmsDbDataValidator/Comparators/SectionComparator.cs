using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class SectionComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Name"], "Section")
{
  public override Task ValidateContent()
  {
    ValidateSections(_dbEntities.OfType<SectionDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateSections(SectionDbEntity[] Sections)
  {
    foreach (var contentfulSection in _contentfulEntities)
    {
      ValidateSection(Sections, contentfulSection);
    }
  }

  private void ValidateSection(SectionDbEntity[] Sections, JsonNode contentfulSection)
  {
    var contentfulSectionId = contentfulSection.GetEntryId();

    if (contentfulSectionId == null)
    {
      Console.WriteLine($"Couldn't find ID for Contentful Section {contentfulSection}");
      return;
    }

    var matchingDbSection = Sections.FirstOrDefault(Section => Section.Id == contentfulSectionId);

    if (matchingDbSection == null)
    {
      Console.WriteLine($"No matching Section found for contentful Section with ID: {contentfulSectionId}");
      return;
    }

    var interstitialPageValidationResult = ValidateChild<SectionDbEntity>(matchingDbSection, @"InterstitialPageId", contentfulSection, "interstitialPage");

    ValidateProperties(contentfulSection, matchingDbSection, interstitialPageValidationResult);

    ValidateChildren(contentfulSection, "questions", matchingDbSection, section => section.Questions);
    ValidateChildren(contentfulSection, "recommendations", matchingDbSection, section => section.Recommendations);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.Sections.Select(section => new SectionDbEntity()
    {
      Id = section.Id,
      Name = section.Name,
      InterstitialPageId = section.InterstitialPageId,
      Questions = section.Questions.Select(question => new QuestionDbEntity()
      {
        Id = question.Id
      }).ToList(),
      Recommendations = section.Recommendations.Select(recommendation => new RecommendationPageDbEntity()
      {
        Id = recommendation.Id
      }).ToList()
    });
  }
}