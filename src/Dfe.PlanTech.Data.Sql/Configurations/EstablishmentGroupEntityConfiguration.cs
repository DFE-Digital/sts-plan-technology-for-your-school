using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class EstablishmentGroupEntityConfiguration
    : IEntityTypeConfiguration<EstablishmentGroupEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentGroupEntity> builder)
    {
        builder.ToTable("establishmentGroup", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Uid).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.GroupName).HasMaxLength(400).IsRequired(false);
        builder.Property(x => x.GroupType).HasMaxLength(400).IsRequired(false);
        builder.Property(x => x.GroupStatus).HasMaxLength(400).IsRequired(false);
    }
}
