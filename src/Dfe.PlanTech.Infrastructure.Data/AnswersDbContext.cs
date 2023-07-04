using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class AnswersDbContext : DbContext, IAnswersDbContext
{
    public DbSet<AnswerDto> Answers { get; set; } = null!;

    public AnswersDbContext() { }
    public AnswersDbContext(DbContextOptions<AnswersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Use Singular table names
        foreach (var entity in modelBuilder.Model.GetEntityTypes()) entity.SetTableName(entity.DisplayName());

        // Setup Answer Table
        modelBuilder.Entity<AnswerDto>(builder =>
        {
            builder.HasKey(answer => answer.Id);
            builder.Property(answer => answer.Id).ValueGeneratedOnAdd();
            builder.Property(answer => answer.AnswerText).HasMaxLength(4000); // NVARCHAR Max Length
            builder.Property(answer => answer.ContentfulRef).HasMaxLength(50);
            builder.Property(answer => answer.DateCreated).ValueGeneratedOnAdd();
        });

    }

    public void AddAnswer(AnswerDto answer) => Answers.Add(answer);

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();
}