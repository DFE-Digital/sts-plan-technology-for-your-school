using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public class SelfAssessmentPage(CmsDbContext db) : ContentGenerator(db)
{
    public override void CreateData()
    {
        db.Pages.Add(new PageDbEntity()
        {
            Id = "self-assessment-id",
            InternalName = "self-assessment-internal-name",
            Slug = "self-assessment",
            Content =
            [
                new HeaderDbEntity()
                {
                    Id = "self-assessment-header-id",
                    Text = "Self Assessment",
                    Tag = HeaderTag.H1,
                    Size = HeaderSize.ExtraLarge
                },
                FindContentById("connectivity-category-id")
            ],
            Title = new TitleDbEntity()
            {
                Id = "self-assessment-title-id",
                Text = "Technology self-assessment",
            },
            DisplayOrganisationName = true,
        });
    }
}