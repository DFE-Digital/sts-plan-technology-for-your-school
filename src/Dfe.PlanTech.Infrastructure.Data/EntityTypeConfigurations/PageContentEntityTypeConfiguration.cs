using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class PageContentEntityTypeConfiguration : IEntityTypeConfiguration<PageContentDbEntity>
{
    public void Configure(EntityTypeBuilder<PageContentDbEntity> builder)
    {
        builder.HasOne(pc => pc.BeforeContentComponent)
                .WithMany()
                .HasForeignKey(pc => pc.BeforeContentComponentId);

        builder.HasOne(pc => pc.ContentComponent)
                .WithMany()
                .HasForeignKey(pc => pc.ContentComponentId);

        builder.HasOne(pc => pc.Page).WithMany().HasForeignKey("PageId");
    }
}
