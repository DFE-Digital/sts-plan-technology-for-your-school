using Dfe.PlanTech.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Configurations;

internal class SectionStatusEntityConfiguration : IEntityTypeConfiguration<SectionStatusEntity>
{
    public void Configure(EntityTypeBuilder<SectionStatusEntity> builder)
    {
        builder.HasNoKey();
    }
}
