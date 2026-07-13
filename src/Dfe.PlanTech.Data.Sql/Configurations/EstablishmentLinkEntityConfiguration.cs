using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class EstablishmentLinkEntityConfiguration
    : IEntityTypeConfiguration<EstablishmentLinkEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentLinkEntity> builder)
    {
        builder.ToTable("establishmentLink", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.GroupUid).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.EstablishmentName).HasMaxLength(400).IsRequired(false);
        builder.Property(x => x.Urn).HasMaxLength(32).IsRequired(false);
    }
}
