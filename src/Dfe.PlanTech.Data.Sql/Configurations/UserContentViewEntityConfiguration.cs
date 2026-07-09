using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

public class UserContentViewEntityConfiguration : IEntityTypeConfiguration<UserContentViewEntity>
{
    public void Configure(EntityTypeBuilder<UserContentViewEntity> builder)
    {
        builder.ToTable("userContentView", "dbo");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ContentfulRef).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UserActionId).IsRequired();
    }
}
