using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

[ExcludeFromCodeCoverage]
internal class SectionStatusEntityConfiguration : IEntityTypeConfiguration<SectionStatusEntity>
{
    public void Configure(EntityTypeBuilder<SectionStatusEntity> builder)
    {
        builder.HasNoKey();
    }
}
