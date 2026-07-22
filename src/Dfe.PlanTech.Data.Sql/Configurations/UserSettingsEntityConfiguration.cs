using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class UserSettingsEntityConfiguration : IEntityTypeConfiguration<UserSettingsEntity>
{
    public void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
    {
        builder.ToTable("userSettings", "dbo");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired(false);
    }
}
