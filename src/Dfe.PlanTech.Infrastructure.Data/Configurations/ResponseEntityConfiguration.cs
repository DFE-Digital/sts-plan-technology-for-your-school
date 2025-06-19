using Dfe.PlanTech.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Configurations;

internal class ResponseEntityConfiguration : IEntityTypeConfiguration<ResponseEntity>
{
    public void Configure(EntityTypeBuilder<ResponseEntity> builder)
    {
        builder.HasKey(response => response.Id);
        builder.ToTable(tb => tb.HasTrigger("tr_response"));
        builder.Property(response => response.DateCreated).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(submission => submission.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
    }
}
