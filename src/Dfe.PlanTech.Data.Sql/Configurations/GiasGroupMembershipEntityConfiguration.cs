using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class GiasGroupMembershipEntityConfiguration
    : IEntityTypeConfiguration<GiasGroupMembershipEntity>
{
    public void Configure(EntityTypeBuilder<GiasGroupMembershipEntity> builder)
    {
        builder.ToTable("groupMembership", "gias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.Urn).IsRequired();
        builder.Property(x => x.GroupUid).IsRequired();
        builder.Property(x => x.SyncedAt).IsRequired();
    }
}
