using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class GiasEstablishmentEntityConfiguration
    : IEntityTypeConfiguration<GiasEstablishmentEntity>
{
    public void Configure(EntityTypeBuilder<GiasEstablishmentEntity> builder)
    {
        builder.ToTable("establishment", "gias");

        builder.HasKey(x => x.Urn);

        builder.Property(x => x.Urn).IsRequired();
        builder.Property(x => x.Uprn).IsRequired(false);
        builder.Property(x => x.EstablishmentNumber).IsRequired(false);
        builder.Property(x => x.EstablishmentName).IsRequired();
        builder.Property(x => x.EstablishmentStatusCode).IsRequired();
        builder.Property(x => x.LocalAuthorityCode).IsRequired();
        builder.Property(x => x.PhaseCode).IsRequired();
        builder.Property(x => x.SyncedAt).IsRequired();
        builder.Property(x => x.Ukprn).IsRequired(false);

        builder
            .HasOne(e => e.TypeOfEstablishment)
            .WithMany()
            .HasForeignKey(e => e.TypeOfEstablishmentCode)
            .HasPrincipalKey(toe => toe.TypeOfEstablishmentCode);
    }
}
