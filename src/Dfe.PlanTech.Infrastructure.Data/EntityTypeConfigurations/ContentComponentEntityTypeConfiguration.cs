using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class ContentComponentEntityTypeConfiguration(ContentfulOptions contentfulOptions) : IEntityTypeConfiguration<ContentComponentDbEntity>
{
    public void Configure(EntityTypeBuilder<ContentComponentDbEntity> builder)
    {
        builder.ToTable("ContentComponents", CmsDbContext.Schema);
        builder.Property(e => e.Id).HasMaxLength(30);

        builder.HasQueryFilter(ShouldShowEntity());
    }

    private Expression<Func<ContentComponentDbEntity, bool>> ShouldShowEntity()
    =>entity => (contentfulOptions.UsePreview || entity.Published) && !entity.Archived && !entity.Deleted;
}
