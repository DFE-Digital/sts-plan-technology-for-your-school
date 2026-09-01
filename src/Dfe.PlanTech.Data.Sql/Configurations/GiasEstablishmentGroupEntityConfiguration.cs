using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class GiasEstablishmentGroupEntityConfiguration
    : IEntityTypeConfiguration<GiasEstablishmentGroupEntity>
{
    public void Configure(EntityTypeBuilder<GiasEstablishmentGroupEntity> builder)
    {
        builder.ToTable("establishmentGroup", "gias");

        builder.HasKey(x => x.GroupUid);

        builder.Property(x => x.GroupUid).IsRequired();
        builder.Property(x => x.GroupId).IsRequired(false);
        builder.Property(x => x.GroupName).IsRequired();
        builder.Property(x => x.GroupStatusCode).IsRequired();
        builder.Property(x => x.GroupTypeCode).IsRequired();
        builder.Property(x => x.SyncedAt).IsRequired();
        builder.Property(x => x.Ukprn).IsRequired(false);
    }
}
