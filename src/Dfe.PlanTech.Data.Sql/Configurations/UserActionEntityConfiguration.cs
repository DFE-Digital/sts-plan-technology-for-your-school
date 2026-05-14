using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

public class UserActionEntityConfiguration : IEntityTypeConfiguration<UserActionEntity>
{
    public void Configure(EntityTypeBuilder<UserActionEntity> builder)
    {
        builder.ToTable("userAction", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasColumnName("userId")
            .IsRequired();

        builder.Property(x => x.EstablishmentId)
            .HasColumnName("establishmentId");

        builder.Property(x => x.MatEstablishmentId)
            .HasColumnName("matEstablishmentId");

        builder.Property(x => x.RequestedUrl)
            .HasColumnName("requestedUrl")
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.DateCreated)
            .HasColumnName("dateCreated")
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();
    }
}
