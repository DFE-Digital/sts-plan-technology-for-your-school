using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class GiasTypeOfEstablishmentEntityConfiguration
    : IEntityTypeConfiguration<GiasTypeOfEstablishmentEntity>
{
    public void Configure(EntityTypeBuilder<GiasTypeOfEstablishmentEntity> builder)
    {
        builder.ToTable("typeOfEstablishment", "gias");

        builder.HasKey(x => x.TypeOfEstablishmentCode);

        builder.Property(x => x.TypeOfEstablishmentCode).IsRequired();
        builder.Property(x => x.TypeOfEstablishmentName).IsRequired();
    }
}
