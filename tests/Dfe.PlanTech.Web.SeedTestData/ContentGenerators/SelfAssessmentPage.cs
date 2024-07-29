using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public class SelfAssessmentPage(CmsDbContext db) : IContentGenerator
{
    public void CreateData()
    {
        var wifiSubtopic = new SectionDbEntity()
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
        };
        var connectivityCategory = new CategoryDbEntity()
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
            Published = true,
            Sections = [wifiSubtopic]
        };

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
                connectivityCategory
            ],
            Title = new TitleDbEntity()
            {
                Id = "self-assessment-title-id",
                Text = "Technology self-assessment",
            },
            DisplayOrganisationName = true,
            Published = true
        });
    }
}