
using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class ButtonWithEntryReferenceLinksEntityTypeConfiguration : IEntityTypeConfiguration<ButtonWithEntryReferenceLinkDbEntity>
{
  public void Configure(EntityTypeBuilder<ButtonWithEntryReferenceLinkDbEntity> builder)
  {
    builder.ToView("SlugsForButtonWithEntryReferences");
    builder.HasKey("ButtonWithEntryReferenceId");
  }
}
