using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class RichTextContentWithSubtopicRecommendationIdConfiguration : IEntityTypeConfiguration<RichTextContentWithSubtopicRecommendationId>
{
    public void Configure(EntityTypeBuilder<RichTextContentWithSubtopicRecommendationId> builder)
    {
        builder.ToView("RichTextContentsBySubtopicRecommendationId");
    }
}
