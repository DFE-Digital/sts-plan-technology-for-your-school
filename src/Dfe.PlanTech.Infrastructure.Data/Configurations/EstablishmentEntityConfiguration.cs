using Dfe.PlanTech.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Configurations;

internal class EstablishmentEntityConfiguration : IEntityTypeConfiguration<EstablishmentEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentEntity> builder)
    {
        builder.HasKey(establishment => establishment.Id);
        builder.ToTable(tb => tb.HasTrigger("tr_establishment"));
        builder.Property(establishment => establishment.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
    }
}
