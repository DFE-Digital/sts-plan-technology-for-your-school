using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Configurations;

internal class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedOnAdd();
        builder.Property(user => user.DfeSignInRef).HasMaxLength(30);
        builder.Property(user => user.DateCreated).ValueGeneratedOnAdd();
        builder.Property(user => user.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();

        builder.ToTable(tb => tb.HasTrigger("tr_user"));
    }
}
