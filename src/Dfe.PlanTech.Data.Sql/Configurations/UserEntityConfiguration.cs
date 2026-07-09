using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("user", "dbo");
        builder.ToTable(tb => tb.HasTrigger("tr_user"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.DfeSignInRef).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DateCreated).ValueGeneratedOnAdd();
        builder.Property(x => x.DateLastUpdated).HasDefaultValue();
    }
}
