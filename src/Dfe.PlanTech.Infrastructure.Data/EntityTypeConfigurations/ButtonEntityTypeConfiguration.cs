using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class ButtonEntityTypeConfiguration : IEntityTypeConfiguration<ButtonDbEntity>
{
    public void Configure(EntityTypeBuilder<ButtonDbEntity> builder)
    {
        builder.ToTable("Button", CmsDbContext.Schema);
    }
}
