using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Configurations;

internal class SignInEntityConfiguration : IEntityTypeConfiguration<SignInEntity>
{
    public void Configure(EntityTypeBuilder<SignInEntity> builder)
    {
        builder.HasKey(signinId => signinId.Id);
        builder.HasOne(signinId => signinId.User)
            .WithMany(signinId => signinId.SignIns)
            .HasForeignKey(signinId => signinId.UserId)
            .IsRequired();

        //When dealing with Establishment add mapping here and remove following lines
        builder.Property(signinId => signinId.EstablishmentId).HasDefaultValue(1);
        //////
    }
}
