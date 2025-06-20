using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Configurations;

internal class GroupReadActivityEntityConfiguration : IEntityTypeConfiguration<GroupReadActivityEntity>
{
    public void Configure(EntityTypeBuilder<GroupReadActivityEntity> builder)
    {
        builder.ToTable("groupReadActivity");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.UserId).HasColumnName("userId");
        builder.Property(b => b.UserEstablishmentId).HasColumnName("establishmentId");
        builder.Property(b => b.SelectedEstablishmentId).HasColumnName("selectedEstablishmentId");
        builder.Property(b => b.SelectedEstablishmentName)
               .HasColumnName("selectedEstablishmentName")
               .HasColumnType("nvarchar(50)")
               .IsRequired();

        builder.Property(b => b.DateSelected)
               .HasColumnName("dateSelected")
               .HasColumnType("datetime")
               .IsRequired();
    }
}
