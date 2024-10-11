
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class ButtonWithEntryReferenceEntityTypeConfiguration : IEntityTypeConfiguration<ButtonWithEntryReferenceDbEntity>
{
    /// <summary>
    /// Configure both as table and as view so that it saves to the table but fetches from the view
    /// </summary>
    public void Configure(EntityTypeBuilder<ButtonWithEntryReferenceDbEntity> builder)
    {
        builder.ToTable("ButtonWithEntryReferences", CmsDbContext.Schema);
        builder.ToView("ButtonWithEntryReferencesWithSlug");
        builder.Navigation(button => button.Button).AutoInclude();
        builder.Property(button => button.LinkType)
            .HasConversion(linkType => linkType.ToString(), linkType => (LinkToEntryType)Enum.Parse(typeof(LinkToEntryType), linkType))
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(button => button.Slug).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
