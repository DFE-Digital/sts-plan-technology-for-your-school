using Dfe.PlanTech.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Configurations;

internal class EstablishmentLinkEntityConfiguration : IEntityTypeConfiguration<EstablishmentEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentEntity> builder)
    {
        builder.HasKey(link => link.Id);
    }
}
