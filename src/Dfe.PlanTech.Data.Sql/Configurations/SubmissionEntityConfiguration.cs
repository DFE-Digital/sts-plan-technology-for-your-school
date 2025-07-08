using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Configurations;

internal class SubmissionEntityConfiguration : IEntityTypeConfiguration<SubmissionEntity>
{
    public void Configure(EntityTypeBuilder<SubmissionEntity> builder)
    {
        builder.HasKey(submission => submission.Id);
        builder.ToTable(tb => tb.HasTrigger("tr_submission"));
        builder.Property(submission => submission.DateCreated).HasColumnType("datetime").HasDefaultValue();
        builder.Property(submission => submission.DateLastUpdated).HasColumnType("datetime").HasDefaultValue();
    }
}
