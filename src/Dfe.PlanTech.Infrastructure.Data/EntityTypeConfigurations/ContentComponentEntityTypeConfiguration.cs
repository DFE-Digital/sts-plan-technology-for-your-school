using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class ContentComponentEntityTypeConfiguration(bool usePreview)
    : IEntityTypeConfiguration<ContentComponentDbEntity>
{
    public void Configure(EntityTypeBuilder<ContentComponentDbEntity> builder)
    {
        builder.ToTable("ContentComponents", CmsDbContext.Schema);
        builder.Property(e => e.Id).HasMaxLength(30);

        builder.HasQueryFilter(entity =>
            (usePreview || entity.Published) && !entity.Archived && !entity.Deleted);
    }
}
