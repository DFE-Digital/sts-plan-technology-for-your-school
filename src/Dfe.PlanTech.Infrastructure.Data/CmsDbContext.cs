using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class CmsDbContext : DbContext
{
  public DbSet<QuestionDbEntity> Questions { get; set; }

  public DbSet<AnswerDbEntity> Answers { get; set; }

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
    modelBuilder.Entity<AnswerDbEntity>(entity =>
    {
      entity.Property(e => e.ContentfulId).HasMaxLength(30);
      entity.HasKey(e => e.Id);
      entity.HasOne(a => a.NextQuestion).WithMany(q => q.PreviousAnswers);
      entity.HasOne(a => a.ParentQuestion).WithMany(q => q.Answers);

      entity.ToTable("Answers", "Contentful", b => b.IsTemporal());
    });

    modelBuilder.Entity<QuestionDbEntity>(entity =>
    {
      entity.Property(e => e.ContentfulId).HasMaxLength(30);
      entity.HasKey(e => e.Id);

      entity.ToTable("Questions", "Contentful", b => b.IsTemporal());
    });
  }
}