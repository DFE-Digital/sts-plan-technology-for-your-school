using Dfe.PlanTech.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Configurations;

internal class EstablishmentGroupEntityConfiguration : IEntityTypeConfiguration<EstablishmentGroupEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroupEntity> builder)
    {
        builder.HasKey(group => group.Id);
    }
}
