using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RichTextContentWithSlugConfiguration : IEntityTypeConfiguration<RichTextContentWithSlugDbEntity>
{
    public void Configure(EntityTypeBuilder<RichTextContentWithSlugDbEntity> builder)
    {
        builder.ToView("RichTextContentsBySlug");
    }
}
