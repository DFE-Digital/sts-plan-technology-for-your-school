using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public class WifiSubtopic(CmsDbContext db) : ContentGenerator(db)
{
    public override void CreateData()
    {
        db.Sections.Add(new SectionDbEntity()
        {
            Name = "Wifi",
            Id = "wifi-section-id",
            InterstitialPage = new PageDbEntity()
            {
                Id = "wifi-interstitial-id",
                InternalName = "wifi-interstitial-name",
                Slug = "wifi",
                Content =
                [
                    new HeaderDbEntity()
                    {
                        Id = "wifi-header-id",
                        Text = "Wifi",
                        Tag = HeaderTag.H3,
                        Size = HeaderSize.Medium
                    }
                ],
                Title = new TitleDbEntity()
                {
                    Id = "wifi-title-id",
                    Text = "Wifi topic",
                }
            }
        });
    }
}