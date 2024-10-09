using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;
[ExcludeFromCodeCoverage]
public class PageEntityTypeConfiguration : IEntityTypeConfiguration<PageDbEntity>
{
    public void Configure(EntityTypeBuilder<PageDbEntity> builder)
    {
        builder.ToTable("Pages", "Contentful");

        builder.HasMany(page => page.BeforeTitleContent)
                .WithMany(c => c.BeforeTitleContentPages)
                .UsingEntity<PageContentDbEntity>(
                  left => left.HasOne(pageContent => pageContent.BeforeContentComponent).WithMany().HasForeignKey("BeforeContentComponentId").OnDelete(DeleteBehavior.Restrict),
                  right => right.HasOne(pageContent => pageContent.Page).WithMany().HasForeignKey("PageId").OnDelete(DeleteBehavior.Restrict)
                );

        builder.HasMany(page => page.Content)
              .WithMany(c => c.ContentPages)
              .UsingEntity<PageContentDbEntity>(
                left => left.HasOne(pageContent => pageContent.ContentComponent).WithMany().HasForeignKey("ContentComponentId").OnDelete(DeleteBehavior.Restrict),
                right => right.HasOne(pageContent => pageContent.Page).WithMany().HasForeignKey("PageId").OnDelete(DeleteBehavior.Restrict)
              );

        builder.HasMany(p => p.AllPageContents).WithOne(pc => pc.Page).HasForeignKey(pc => pc.PageId);

        builder.HasOne(page => page.Title).WithMany(title => title.Pages).OnDelete(DeleteBehavior.Restrict);
    }
}
