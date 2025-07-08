using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Configurations;

internal class EstablishmentGroupEntityConfiguration : IEntityTypeConfiguration<EstablishmentGroupEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroupEntity> builder)
    {
        builder.HasKey(group => group.Id);
    }
}
