using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class WarningComponentEntityTypeConfiguration : IEntityTypeConfiguration<WarningComponentDbEntity>
{
    public void Configure(EntityTypeBuilder<WarningComponentDbEntity> builder)
    {
        builder.HasOne(warning => warning.Text).WithMany(text => text.Warnings).OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(warningComponent => warningComponent.Text).AutoInclude();
    }
}
