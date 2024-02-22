using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class CmsDbContext : DbContext, ICmsDbContext
{
    private const string Schema = "Contentful";

    public DbSet<AnswerDbEntity> Answers { get; set; }

    public DbSet<ButtonDbEntity> Buttons { get; set; }

    public DbSet<ButtonWithEntryReferenceDbEntity> ButtonWithEntryReferences { get; set; }

    public DbSet<ButtonWithLinkDbEntity> ButtonWithLinks { get; set; }

    public DbSet<CategoryDbEntity> Categories { get; set; }

    public DbSet<ComponentDropDownDbEntity> ComponentDropDowns { get; set; }

    public DbSet<ContentComponentDbEntity> ContentComponents { get; set; }

    public DbSet<HeaderDbEntity> Headers { get; set; }

    public DbSet<InsetTextDbEntity> InsetTexts { get; set; }

    public DbSet<NavigationLinkDbEntity> NavigationLink { get; set; }

    public DbSet<PageDbEntity> Pages { get; set; }

    public DbSet<PageContentDbEntity> PageContents { get; set; }

    public DbSet<QuestionDbEntity> Questions { get; set; }

    public DbSet<RecommendationPageDbEntity> RecommendationPages { get; set; }

    public DbSet<RecommendationChunkDbEntity> RecommendationChunks { get; set; }

    public DbSet<RecommendationIntroDbEntity> RecommendationIntros { get; set; }

    public DbSet<RecommendationSectionDbEntity> RecommendationSections { get; set; }

    public DbSet<SubTopicRecommendationDbEntity> SubtopicRecommendations { get; set; }

    public DbSet<RichTextContentDbEntity> RichTextContents { get; set; }

    public DbSet<RichTextContentWithSlugDbEntity> RichTextContentWithSlugs { get; set; }

    public DbSet<RichTextDataDbEntity> RichTextDataDbEntity { get; set; }

    public DbSet<RichTextMarkDbEntity> RichTextMarkDbEntity { get; set; }

    public DbSet<SectionDbEntity> Sections { get; set; }

    public DbSet<TextBodyDbEntity> TextBodies { get; set; }

    public DbSet<TitleDbEntity> Titles { get; set; }

    public DbSet<WarningComponentDbEntity> Warnings { get; set; }

    IQueryable<AnswerDbEntity> ICmsDbContext.Answers => Answers;
    IQueryable<ButtonDbEntity> ICmsDbContext.Buttons => Buttons;
    IQueryable<ButtonWithEntryReferenceDbEntity> ICmsDbContext.ButtonWithEntryReferences => ButtonWithEntryReferences;
    IQueryable<ButtonWithLinkDbEntity> ICmsDbContext.ButtonWithLinks => ButtonWithLinks;
    IQueryable<CategoryDbEntity> ICmsDbContext.Categories => Categories;
    IQueryable<ComponentDropDownDbEntity> ICmsDbContext.ComponentDropDowns => ComponentDropDowns;
    IQueryable<HeaderDbEntity> ICmsDbContext.Headers => Headers;
    IQueryable<InsetTextDbEntity> ICmsDbContext.InsetTexts => InsetTexts;
    IQueryable<NavigationLinkDbEntity> ICmsDbContext.NavigationLink => NavigationLink;
    IQueryable<PageDbEntity> ICmsDbContext.Pages => Pages;
    IQueryable<PageContentDbEntity> ICmsDbContext.PageContents => PageContents;
    IQueryable<QuestionDbEntity> ICmsDbContext.Questions => Questions;
    IQueryable<RecommendationPageDbEntity> ICmsDbContext.RecommendationPages => RecommendationPages;
    IQueryable<RecommendationChunkDbEntity> ICmsDbContext.RecommendationChunks => RecommendationChunks;
    IQueryable<RecommendationIntroDbEntity> ICmsDbContext.RecommendationIntros => RecommendationIntros;
    IQueryable<SubTopicRecommendationDbEntity> ICmsDbContext.SubtopicRecommendations => SubtopicRecommendations;
    IQueryable<RecommendationSectionDbEntity> ICmsDbContext.RecommendationSections => RecommendationSections;

    IQueryable<RichTextContentDbEntity> ICmsDbContext.RichTextContents => RichTextContents;
    IQueryable<RichTextContentWithSlugDbEntity> ICmsDbContext.RichTextContentWithSlugs => RichTextContentWithSlugs
                                                                                                .Include(rt => rt.Data)
                                                                                                .Include(rt => rt.Marks);
    IQueryable<RichTextDataDbEntity> ICmsDbContext.RichTextDataDbEntity => RichTextDataDbEntity;
    IQueryable<RichTextMarkDbEntity> ICmsDbContext.RichTextMarkDbEntity => RichTextMarkDbEntity;
    IQueryable<SectionDbEntity> ICmsDbContext.Sections => Sections;
    IQueryable<TextBodyDbEntity> ICmsDbContext.TextBodies => TextBodies;
    IQueryable<TitleDbEntity> ICmsDbContext.Titles => Titles;
    IQueryable<WarningComponentDbEntity> ICmsDbContext.Warnings => Warnings;

    private readonly ContentfulOptions _contentfulOptions;

    public CmsDbContext()
    {
        _contentfulOptions = new ContentfulOptions(false);
    }

    public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options)
    {
        _contentfulOptions = this.GetService<ContentfulOptions>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<ContentComponentDbEntity>(entity =>
        {
            entity.ToTable("ContentComponents", Schema);
            entity.Property(e => e.Id).HasMaxLength(30);
            entity.HasQueryFilter(ShouldShowEntity());
        });

        modelBuilder.Entity<AnswerDbEntity>(entity =>
        {
            entity.HasOne(a => a.NextQuestion).WithMany(q => q.PreviousAnswers).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.ParentQuestion).WithMany(q => q.Answers).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ButtonWithEntryReferenceDbEntity>().Navigation(button => button.Button).AutoInclude();

        modelBuilder.Entity<ButtonWithLinkDbEntity>().Navigation(button => button.Button).AutoInclude();

        modelBuilder.Entity<RecommendationPageDbEntity>(entity =>
        {
            entity.HasOne(recommendation => recommendation.Page)
                    .WithOne(page => page.RecommendationPage)
                    .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RichTextContentWithSlugDbEntity>().ToView("RichTextContentsBySlug");

        modelBuilder.Entity<WarningComponentDbEntity>(entity =>
        {
            entity.HasOne(warning => warning.Text)
                .WithMany(text => text.Warnings)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Navigation(warningComponent => warningComponent.Text).AutoInclude();
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmsDbContext).Assembly);
    }

    /// <summary>
    /// Should the given entity be displayed? I.e. is it not archived, not deleted, and either published or use preview mode is enabled
    /// </summary>
    private Expression<Func<ContentComponentDbEntity, bool>> ShouldShowEntity()
        => entity => (_contentfulOptions.UsePreview || entity.Published) && !entity.Archived && !entity.Deleted;

    public Task<PageDbEntity?> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
        => Pages.Include(page => page.BeforeTitleContent)
                .Include(page => page.Content)
                .Include(page => page.Title)
                .AsSplitQuery()
                .FirstOrDefaultAsync(page => page.Slug == slug, cancellationToken);

    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
        => queryable.ToListAsync(cancellationToken: cancellationToken);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    => queryable.FirstOrDefaultAsync(cancellationToken);
}