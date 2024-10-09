using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class RichTextContentEntityTypeConfiguration : IEntityTypeConfiguration<RichTextContentDbEntity>
{
    public void Configure(EntityTypeBuilder<RichTextContentDbEntity> builder)
    {
        builder.ToTable("RichTextContents", CmsDbContext.Schema);
        builder.HasOne(rt => rt.Parent).WithMany(text => text.Content).HasForeignKey(rt => rt.ParentId);
    }
}
