
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
        builder.ToView("ButtonWithEntryReferencesWithSlug");
        builder.Navigation(button => button.Button).AutoInclude();
        builder.Property(button => button.LinkType)
            .HasConversion(linkType => linkType.ToString(), linkType => (LinkToEntryType)Enum.Parse(typeof(LinkToEntryType), linkType))
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(button => button.Slug).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
