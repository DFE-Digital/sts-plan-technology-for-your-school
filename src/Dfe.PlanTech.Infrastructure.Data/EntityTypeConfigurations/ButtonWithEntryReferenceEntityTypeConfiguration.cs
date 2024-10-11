
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class ButtonWithEntryReferenceEntityTypeConfiguration : IEntityTypeConfiguration<ButtonWithEntryReferenceDbEntity>
{
    public void Configure(EntityTypeBuilder<ButtonWithEntryReferenceDbEntity> builder)
    {
        builder.ToTable("ButtonWithEntryReferences", CmsDbContext.Schema);
        builder.Navigation(button => button.Button).AutoInclude();
        builder.Navigation(button => button.Link).AutoInclude();
    }
}
