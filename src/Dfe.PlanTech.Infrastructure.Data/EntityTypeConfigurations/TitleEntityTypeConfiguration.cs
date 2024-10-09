using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class TitleEntityTypeConfiguration : IEntityTypeConfiguration<TitleDbEntity>
{
    public void Configure(EntityTypeBuilder<TitleDbEntity> builder)
    {
        builder.ToTable("Titles", CmsDbContext.Schema);
    }
}
