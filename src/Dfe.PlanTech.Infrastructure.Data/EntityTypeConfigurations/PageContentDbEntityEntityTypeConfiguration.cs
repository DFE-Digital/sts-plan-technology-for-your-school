using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class PageContentDbEntityEntityTypeConfiguration : IEntityTypeConfiguration<PageContentDbEntity>
{
  public void Configure(EntityTypeBuilder<PageContentDbEntity> builder)
  {
    builder.HasOne(pc => pc.BeforeContentComponent)
           .WithMany(c => c.BeforeTitleContentPagesJoins);

    builder.HasOne(pc => pc.ContentComponent)
            .WithMany(c => c.ContentPagesJoins);

    builder.HasOne(pc => pc.Page)
            .WithMany(p => p.AllPageContents);
  }
}