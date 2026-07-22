using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class SignInEntityConfiguration : IEntityTypeConfiguration<SignInEntity>
{
    public void Configure(EntityTypeBuilder<SignInEntity> builder)
    {
        builder.ToTable("signIn", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EstablishmentId).IsRequired(false);
        builder
            .Property(x => x.SignInDateTime)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        builder
            .HasOne(x => x.User)
            .WithMany(user => user.SignIns)
            .HasForeignKey(x => x.UserId)
            .IsRequired();

        //When dealing with Establishment add mapping here and remove following lines
        builder.Property(x => x.EstablishmentId).HasDefaultValue(1);
        //////
    }
}
