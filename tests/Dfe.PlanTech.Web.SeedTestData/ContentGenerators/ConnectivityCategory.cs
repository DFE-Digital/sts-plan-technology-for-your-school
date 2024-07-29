using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public class ConnectivityCategory(CmsDbContext db) : ContentGenerator(db)
{
    public override void CreateData()
    {
        db.Categories.Add(new CategoryDbEntity()
        {
            InternalName = "Connectivity",
            Id = "connectivity-category-id",
            Header = new HeaderDbEntity()
            {
                Id = "connectivity-header-id",
                Text = "Connectivity",
                Tag = HeaderTag.H2,
                Size = HeaderSize.Large
            },
            Sections =
            [
                FindContentById("wifi-section-id") as SectionDbEntity,
            ]
        });
    }
}