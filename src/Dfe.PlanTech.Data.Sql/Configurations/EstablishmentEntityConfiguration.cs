using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class EstablishmentEntityConfiguration : IEntityTypeConfiguration<EstablishmentEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentEntity> builder)
    {
        builder.ToTable("establishment", "dbo");
        builder.ToTable(tb => tb.HasTrigger("tr_establishment"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EstablishmentRef).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EstablishmentType).HasMaxLength(50).IsRequired(false);
        builder.Property(x => x.OrgName).HasMaxLength(200).IsRequired(false);
        builder.Property(x => x.DateCreated).ValueGeneratedOnAdd();
        builder.Property(x => x.DateLastUpdated).HasDefaultValue();
        builder.Property(x => x.GroupUid).HasMaxLength(50).IsRequired(false);
    }
}
