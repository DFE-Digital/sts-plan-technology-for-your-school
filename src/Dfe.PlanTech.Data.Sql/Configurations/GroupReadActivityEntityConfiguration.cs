using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class GroupReadActivityEntityConfiguration
    : IEntityTypeConfiguration<GroupReadActivityEntity>
{
    public void Configure(EntityTypeBuilder<GroupReadActivityEntity> builder)
    {
        builder.ToTable("groupReadActivity", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.UserEstablishmentId).HasColumnName("establishmentId").IsRequired();
        builder.Property(x => x.SelectedEstablishmentId).IsRequired();
        builder.Property(x => x.SelectedEstablishmentName).HasMaxLength(400).IsRequired();
        builder.Property(x => x.DateSelected).IsRequired();
        builder.Property(x => x.UserActionId).IsRequired(false);
    }
}
