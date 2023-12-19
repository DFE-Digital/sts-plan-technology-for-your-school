using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class CmsDbContext : DbContext
{
    private const string Schema = "Contentful";

    public DbSet<AnswerDbEntity> Answers { get; set; }

    public DbSet<ButtonDbEntity> Buttons { get; set; }

    public DbSet<ButtonWithEntryReferenceDbEntity> ButtonWithEntryReferences { get; set; }

    public DbSet<ButtonWithLinkDbEntity> ButtonWithLinks { get; set; }

    public DbSet<CategoryDbEntity> Categories { get; set; }

    public DbSet<ComponentDropDownDbEntity> ComponentDropDowns { get; set; }

    public DbSet<HeaderDbEntity> Headers { get; set; }

    public DbSet<InsetTextDbEntity> InsetTexts { get; set; }

    public DbSet<NavigationLinkDbEntity> NavigationLink { get; set; }

    public DbSet<PageDbEntity> Pages { get; set; }

    public DbSet<PageContentDbEntity> PageContents { get; set; }

    public DbSet<QuestionDbEntity> Questions { get; set; }

    public DbSet<RecommendationPageDbEntity> RecommendationPages { get; set; }

    public DbSet<RichTextContentDbEntity> RichTextContents { get; set; }

    public DbSet<SectionDbEntity> Sections { get; set; }

    public DbSet<TextBodyDbEntity> TextBodies { get; set; }

    public DbSet<TitleDbEntity> Titles { get; set; }

    public DbSet<WarningComponentDbEntity> Warnings { get; set; }

    public CmsDbContext() { }

    public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<ContentComponentDbEntity>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(30);

            entity.ToTable("ContentComponents", Schema);
        });

        modelBuilder.Entity<AnswerDbEntity>(entity =>
        {
            entity.HasOne(a => a.NextQuestion).WithMany(q => q.PreviousAnswers).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.ParentQuestion).WithMany(q => q.Answers).OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("Answers", Schema);
        });

        modelBuilder.Entity<CategoryDbEntity>(entity =>
        {
            entity.HasMany(category => category.Sections)
              .WithOne(section => section.Category)
              .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("Categories", Schema);
        });

        modelBuilder.Entity<PageDbEntity>(entity =>
        {
            entity.HasMany(page => page.BeforeTitleContent)
              .WithMany(c => c.BeforeTitleContentPages)
              .UsingEntity<PageContentDbEntity>(
                left => left.HasOne(pageContent => pageContent.BeforeContentComponent).WithMany().HasForeignKey("BeforeContentComponentId").OnDelete(DeleteBehavior.Restrict),
                right => right.HasOne(pageContent => pageContent.Page).WithMany().HasForeignKey("PageId").OnDelete(DeleteBehavior.Restrict)
              );

            entity.HasMany(page => page.Content)
              .WithMany(c => c.ContentPages)
              .UsingEntity<PageContentDbEntity>(
                left => left.HasOne(pageContent => pageContent.ContentComponent).WithMany().HasForeignKey("ContentComponentId").OnDelete(DeleteBehavior.Restrict),
                right => right.HasOne(pageContent => pageContent.Page).WithMany().HasForeignKey("PageId").OnDelete(DeleteBehavior.Restrict)
              );

            entity.HasOne(page => page.Title).WithMany(title => title.Pages).OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("Pages", Schema);
        });

        modelBuilder.Entity<QuestionDbEntity>(entity =>
        {
            entity.ToTable("Questions", Schema);
        });

        modelBuilder.Entity<RecommendationPageDbEntity>(entity =>
        {
            entity.HasOne(recommendation => recommendation.Page)
            .WithOne(page => page.RecommendationPage)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SectionDbEntity>(entity =>
        {
            entity.HasOne(section => section.InterstitialPage)
            .WithOne(page => page.Section)
            .HasForeignKey<SectionDbEntity>()
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(section => section.Category)
            .WithMany(category => category.Sections)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(section => section.Questions)
            .WithOne(question => question.Section)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TitleDbEntity>(entity =>
        {
            entity.ToTable("Titles", Schema);
        });

        modelBuilder.Entity<WarningComponentDbEntity>(entity =>
        {
            entity.HasOne(warning => warning.Text).WithMany(text => text.Warnings).OnDelete(DeleteBehavior.Restrict);
        });
    }
}