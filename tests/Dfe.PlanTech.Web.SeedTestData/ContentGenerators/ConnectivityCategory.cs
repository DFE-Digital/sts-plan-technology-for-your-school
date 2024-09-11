using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

/// <summary>
/// Generic Category with complete data
/// </summary>
public class ConnectivityCategory(CmsDbContext db) : ContentGenerator
{
    private static SectionDbEntity GetWifiSubtopic()
    {
        var q2 = CreateComponent(new QuestionDbEntity()
        {
            Slug = "wifi-q2",
            Text = "Do you have someone responsible for reviewing the broadband?",
            Order = 1,
            Answers =
            [
                CreateComponent(new AnswerDbEntity()
                {
                    Text = "Yes",
                    Maturity = "High",
                }),
                CreateComponent(new AnswerDbEntity()
                {
                    Text = "No",
                    Maturity = "Low",
                }),
            ]
        });
        var q1 = CreateComponent(new QuestionDbEntity
        {
            Slug = "wifi-q1",
            Text = "What type of broadband do you have?",
            Order = 0,
            Answers =
            [
                CreateComponent(new AnswerDbEntity()
                {
                    Text = "Fibre",
                    Maturity = "High",
                    NextQuestion = q2
                }),
                CreateComponent(new AnswerDbEntity()
                {
                    Text = "Mobile",
                    Maturity = "Medium",
                    NextQuestion = q2
                }),
            ]
        });

        return CreateComponent(new SectionDbEntity
        {
            Name = "Wifi",
            InterstitialPage = CreateComponent(new PageDbEntity
            {
                InternalName = "wifi-interstitial-name",
                Slug = "wifi",
                Content =
                [
                    CreateComponent(new HeaderDbEntity
                    {
                        Text = "Wifi",
                        Tag = HeaderTag.H3,
                        Size = HeaderSize.Medium,
                    }),
                    CreateComponent(new ButtonWithEntryReferenceDbEntity
                    {
                        Button = CreateComponent(new ButtonDbEntity { Value = "Continue" }),
                        LinkToEntry = q1,
                    })
                ],
                Title = CreateComponent(new TitleDbEntity { Text = "Wifi topic" }),
            }),
            Order = 0,
            Questions = [q1, q2]
        });
    }

    public override void CreateData()
    {
        var wifiSubtopic = GetWifiSubtopic();
        db.Categories.Add(CreateComponent(CreateComponent(new CategoryDbEntity
        {
            InternalName = "Connectivity",
            Header = CreateComponent(new HeaderDbEntity
            {
                Text = "Connectivity",
                Tag = HeaderTag.H2,
                Size = HeaderSize.Large,
            }),
            Sections = [wifiSubtopic],
            Order = 0
        })));
    }
}
