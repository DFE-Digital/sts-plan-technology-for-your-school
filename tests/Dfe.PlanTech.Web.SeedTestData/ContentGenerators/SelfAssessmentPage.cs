using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

public class SelfAssessmentPage(CmsDbContext db) : IContentGenerator
{
    private SectionDbEntity GetWifiSubtopic()
    {
        var q2 = new QuestionDbEntity
        {
            Id = "wifi-q2",
            Published = true,
            Slug = "wifi-q2",
            Text = "Do you have someone responsible for reviewing the broadband?",
            Order = 1,
            Answers =
            [
                new AnswerDbEntity
                {
                    Text = "Yes",
                    Maturity = "High",
                    Published = true,
                    Id = "wifi-q2-a1"
                },
                new AnswerDbEntity
                {
                    Text = "No",
                    Maturity = "Low",
                    Published = true,
                    Id = "wifi-q2-a2"
                }
            ]
        };
        var q1 = new QuestionDbEntity
        {
            Id = "wifi-q1",
            Published = true,
            Slug = "wifi-q1",
            Text = "What type of broadband do you have?",
            Order = 0,
            Answers =
            [
                new AnswerDbEntity
                {
                    Text = "Fibre",
                    Maturity = "High",
                    NextQuestion = q2,
                    Published = true,
                    Id = "wifi-q1-a1"
                },
                new AnswerDbEntity
                {
                    Text = "Mobile",
                    Maturity = "Medium",
                    NextQuestion = q2,
                    Published = true,
                    Id = "wifi-q1-a2"
                }
            ]
        };
        var interstitialHeader = new HeaderDbEntity
        {
            Id = "wifi-header-id",
            Text = "Wifi",
            Tag = HeaderTag.H3,
            Size = HeaderSize.Medium,
            Published = true
        };
        var interstitialContinueButton = new ButtonWithEntryReferenceDbEntity
        {
            Id = "wifi-button-reference-id",
            Button = new ButtonDbEntity
            {
                Id = "wifi-continue-button",
                Published = true,
                Value = "Continue"
            },
            LinkToEntry = q1,
            Published = true
        };
        return new SectionDbEntity
        {
            Name = "Wifi",
            Id = "wifi-section-id",
            InterstitialPage = new PageDbEntity
            {
                Id = "wifi-interstitial-id",
                InternalName = "wifi-interstitial-name",
                Slug = "wifi",
                Content = [interstitialHeader, interstitialContinueButton],
                Title = new TitleDbEntity
                {
                    Id = "wifi-title-id",
                    Text = "Wifi topic",
                    Published = true
                },
                Published = true,
            },
            Order = 0,
            Published = true,
            Questions = [q1, q2]
        };
    }

    private CategoryDbEntity GetConnectivityCategory()
    {
        var wifiSubtopic = GetWifiSubtopic();
        return new CategoryDbEntity
        {
            InternalName = "Connectivity",
            Id = "connectivity-category-id",
            Header = new HeaderDbEntity
            {
                Id = "connectivity-header-id",
                Text = "Connectivity",
                Tag = HeaderTag.H2,
                Size = HeaderSize.Large,
                Published = true
            },
            Published = true,
            Sections = [wifiSubtopic]
        };
    }

    public void CreateData()
    {
        var connectivityCategory = GetConnectivityCategory();

        db.Pages.Add(new PageDbEntity
        {
            Id = "self-assessment-id",
            InternalName = "self-assessment-internal-name",
            Slug = "self-assessment",
            Content =
            [
                new HeaderDbEntity
                {
                    Id = "self-assessment-header-id",
                    Text = "Self Assessment",
                    Tag = HeaderTag.H1,
                    Size = HeaderSize.ExtraLarge,
                    Published = true
                },
                connectivityCategory
            ],
            Title = new TitleDbEntity
            {
                Id = "self-assessment-title-id",
                Text = "Technology self-assessment",
                Published = true
            },
            DisplayOrganisationName = true,
            Published = true
        });
    }
}