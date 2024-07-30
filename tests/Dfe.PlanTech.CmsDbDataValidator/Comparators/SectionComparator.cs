using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
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

    private void ValidateSection(SectionDbEntity[] sections, JsonNode contentfulSection)
    {
        var matchingDbSection = TryRetrieveMatchingDbEntity(sections, contentfulSection);

        if (matchingDbSection == null)
        {
            return;
        }

        ValidateProperties(contentfulSection, matchingDbSection, GetValidationErrors(matchingDbSection, contentfulSection).ToArray());
    }


    protected IEnumerable<DataValidationError> GetValidationErrors(SectionDbEntity databaseSection, JsonNode contentfulSection)
    {
        var interstitialPageValidationResult = ValidateChild<SectionDbEntity>(databaseSection, "InterstitialPageId", contentfulSection, "interstitialPage");

        if (interstitialPageValidationResult != null)
        {
            yield return new DataValidationError("InterstitialPage", interstitialPageValidationResult);
        }

        foreach (var child in ValidateChildren(contentfulSection, "questions", databaseSection, dbRecommendationChunk => dbRecommendationChunk.Questions))
        {
            yield return child;
        }
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
        });
    }
}
