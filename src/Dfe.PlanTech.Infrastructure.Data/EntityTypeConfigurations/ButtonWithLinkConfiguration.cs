using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class ButtonWithLinkConfiguration : IEntityTypeConfiguration<ButtonWithLinkDbEntity>
{
    public void Configure(EntityTypeBuilder<ButtonWithLinkDbEntity> builder)
    {
        builder.Navigation(button => button.Button).AutoInclude();
    }
}
