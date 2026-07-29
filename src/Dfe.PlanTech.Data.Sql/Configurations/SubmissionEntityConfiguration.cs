using Dfe.PlanTech.Data.Sql.Common;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class SubmissionEntityConfiguration : IEntityTypeConfiguration<SubmissionEntity>
{
    public void Configure(EntityTypeBuilder<SubmissionEntity> builder)
    {
        builder.ToTable("submission", "dbo");
        builder.ToTable(tb => tb.HasTrigger("tr_submission"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EstablishmentId).IsRequired();
        builder.Property(x => x.SectionId).IsRequired();
        builder.Property(x => x.SectionName).IsRequired();
        builder.Property(x => x.DateCreated).HasDefaultValue();
        builder.Property(x => x.DateLastUpdated).HasDefaultValue();
        builder.Property(x => x.DateCompleted).IsRequired(false);
        builder.Property(x => x.Deleted).IsRequired();
        builder
            .Property(x => x.Status)
            .HasMaxLength(50)
            .HasConversion(StatusConverters.SubmissionStatusConverter)
            .IsRequired();
        builder.Property(x => x.UserActionId).IsRequired(false);
        builder.Property(x => x.CreatedUserActionId).IsRequired(false);
        builder.Property(x => x.LastUpdatedUserActionId).IsRequired(false);
        builder.Property(x => x.CompletedUserActionId).IsRequired(false);
    }
}
