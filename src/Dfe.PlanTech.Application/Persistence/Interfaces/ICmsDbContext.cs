using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface ICmsDbContext
{
    public IQueryable<AnswerDbEntity> Answers { get; }

    public IQueryable<ButtonDbEntity> Buttons { get; }

    public IQueryable<ButtonWithEntryReferenceDbEntity> ButtonWithEntryReferences { get; }

    public IQueryable<ButtonWithLinkDbEntity> ButtonWithLinks { get; }

    public IQueryable<CategoryDbEntity> Categories { get; }

    public IQueryable<ComponentDropDownDbEntity> ComponentDropDowns { get; }

    public IQueryable<HeaderDbEntity> Headers { get; }

    public IQueryable<InsetTextDbEntity> InsetTexts { get; }

    public IQueryable<NavigationLinkDbEntity> NavigationLink { get; }

    public IQueryable<PageDbEntity> Pages { get; }

    public IQueryable<PageContentDbEntity> PageContents { get; }

    public IQueryable<QuestionDbEntity> Questions { get; }

    public IQueryable<RecommendationPageDbEntity> RecommendationPages { get; }

    public IQueryable<RichTextContentDbEntity> RichTextContents { get; }
    public IQueryable<RichTextContentWithSlugDbEntity> RichTextContentWithSlugs { get; }

    public IQueryable<RichTextDataDbEntity> RichTextDataDbEntity { get; }

    public IQueryable<RichTextMarkDbEntity> RichTextMarkDbEntity { get; }

    public IQueryable<SectionDbEntity> Sections { get; }

    public IQueryable<TextBodyDbEntity> TextBodies { get; }

    public IQueryable<TitleDbEntity> Titles { get; }

    public IQueryable<WarningComponentDbEntity> Warnings { get; }


    public Task<PageDbEntity?> GetPageBySlug(string slug, CancellationToken cancellationToken = default);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
}