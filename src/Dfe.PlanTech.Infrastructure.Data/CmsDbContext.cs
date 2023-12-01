using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class CmsDbContext : DbContext
{
  public DbSet<AnswerDbEntity> Answers { get; set; }

  public DbSet<PageDbEntity> Pages { get; set; }

  public DbSet<QuestionDbEntity> Questions { get; set; }

  public DbSet<TitleDbEntity> Titles { get; set; }

  public CmsDbContext() { }

  public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options)
  {

  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    base.OnConfiguring(optionsBuilder);
    optionsBuilder.UseSqlServer("CONNECTION STRING");
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<ContentComponentDbEntity>(entity =>
    {
      entity.Property(e => e.Id).HasMaxLength(30);
    });

    modelBuilder.Entity<AnswerDbEntity>(entity =>
    {
      entity.HasOne(a => a.NextQuestion).WithMany(q => q.PreviousAnswers);
      entity.HasOne(a => a.ParentQuestion).WithMany(q => q.Answers);

      entity.ToTable("Answers", "Contentful");
    });

    modelBuilder.Entity<PageDbEntity>(entity =>
    {
      entity.HasMany(page => page.BeforeTitleContent).WithMany(c => c.BeforeTitleContentPages);
      entity.HasMany(page => page.Content).WithMany(c => c.ContentPages);

      entity.ToTable("Pages", "Contentful");
    });

    modelBuilder.Entity<QuestionDbEntity>(entity =>
    {
      entity.ToTable("Questions", "Contentful");
    });

    modelBuilder.Entity<TitleDbEntity>(entity =>
    {
      entity.HasMany(title => title.Pages).WithOne(p => p.Title);
      entity.ToTable("Titles", "Contentful");
    });
  }
}