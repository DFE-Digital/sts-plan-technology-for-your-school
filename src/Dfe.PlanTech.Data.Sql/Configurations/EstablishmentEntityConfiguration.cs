using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class EstablishmentEntityConfiguration : IEntityTypeConfiguration<EstablishmentEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentEntity> builder)
    {
        builder.HasKey(establishment => establishment.Id);
        builder.ToTable(tb => tb.HasTrigger("tr_establishment"));
        builder.Property(establishment => establishment.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
    }
}
