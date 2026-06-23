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

        builder.Property(x => x.Id).ValueGeneratedNever().IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EstablishmentId).IsRequired(false);
        builder.Property(x => x.MatEstablishmentId).IsRequired(false);
        builder.Property(x => x.RequestedUrl).HasMaxLength(2048).IsRequired();
        builder
            .Property(x => x.DateCreated)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder
            .Property(x => x.SessionId)
            .HasColumnName("sessionId")
            .HasColumnType("uniqueidentifier")
            .IsRequired(false);
    }
}
