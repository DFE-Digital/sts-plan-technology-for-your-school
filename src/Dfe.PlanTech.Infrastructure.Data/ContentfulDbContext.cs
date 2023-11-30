

using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data;

public class ContentfulDbContext : DbContext
{
  public DbSet<QuestionDbEntity> Questions { get; set; }

  public DbSet<AnswerDbEntity> Answers { get; set; }

  public ContentfulDbContext()
  {

  }

  public ContentfulDbContext(DbContextOptions<ContentfulDbContext> options) : base(options)
  {

  }
}