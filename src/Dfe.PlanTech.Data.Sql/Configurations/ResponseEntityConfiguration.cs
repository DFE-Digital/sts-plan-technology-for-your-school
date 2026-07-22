using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class ResponseEntityConfiguration : IEntityTypeConfiguration<ResponseEntity>
{
    public void Configure(EntityTypeBuilder<ResponseEntity> builder)
    {
        builder.ToTable("response", "dbo");
        builder.ToTable(tb => tb.HasTrigger("tr_response"));

        builder.HasKey(response => response.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.SubmissionId).IsRequired();
        builder.Property(x => x.QuestionId).IsRequired();
        builder.Property(x => x.AnswerId).IsRequired();
        builder.Property(x => x.DateCreated).ValueGeneratedOnAdd();
        builder.Property(x => x.DateLastUpdated).HasDefaultValue();
        builder.Property(x => x.UserEstablishmentId).IsRequired(false);
        builder.Property(x => x.UserActionId).IsRequired(false);
    }
}
