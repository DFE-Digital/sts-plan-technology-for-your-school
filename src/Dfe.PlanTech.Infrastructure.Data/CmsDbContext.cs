using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class CmsDbContext : DbContext
{
  public DbSet<JsonCmsDbEntity> ContentJson { get; private set; }

  public CmsDbContext() { }

  public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options) { }


  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<JsonCmsDbEntity>(entity =>
    {
      entity.UseTpcMappingStrategy()
              .ToTable("JsonEntities", "Contentful", b => b.IsTemporal());

      entity.Property(e => e.ContentTypeId).HasMaxLength(50);
      entity.Property(e => e.ContentId).HasMaxLength(30);
      entity.HasKey(e => e.Id);
    });
  }
}