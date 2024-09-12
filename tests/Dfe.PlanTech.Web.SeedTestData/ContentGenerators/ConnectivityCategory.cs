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
    private SectionDbEntity GetWifiSubtopic()
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
        var fibreAnswer = CreateComponent(new AnswerDbEntity()
        {
            Text = "Fibre",
            Maturity = "Low",
            NextQuestion = q2
        });
        var mobileAnswer = CreateComponent(new AnswerDbEntity()
        {
            Text = "Mobile",
            Maturity = "Low",
            NextQuestion = q2
        });
        var q1 = CreateComponent(new QuestionDbEntity
        {
            Slug = "wifi-q1",
            Text = "What type of broadband do you have?",
            Order = 0,
            Answers = [fibreAnswer, mobileAnswer]
        });

        var recommendationSection = CreateComponent(new RecommendationSectionDbEntity()
        {
            Chunks =
            [
                CreateComponent(new RecommendationChunkDbEntity()
                {
                    Content = [CreateTextBody("recommendation chunk fibre contents")],
                    Answers = [fibreAnswer],
                    Header = "Recommendation for fibre"
                }),
                CreateComponent(new RecommendationChunkDbEntity()
                {
                    Content = [CreateTextBody("recommendation chunk mobile contents")],
                    Answers = [mobileAnswer],
                    Header = "Recommendation for mobile"
                })
            ]
        });

        var subtopic = CreateComponent(new SectionDbEntity
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

        db.SubtopicRecommendations.Add(CreateComponent(new SubtopicRecommendationDbEntity()
        {
            Intros =
            [
                CreateComponent(new RecommendationIntroDbEntity()
                {
                    Slug = "wifi-recommendation-intro",
                    Maturity = "Low",
                    Header = CreateComponent(new HeaderDbEntity()
                    {
                        Text = "Wifi recommendation intro",
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large
                    }),
                    Content = [CreateTextBody("recommendation intro text")]
                })
            ],
            Section = recommendationSection,
            Subtopic = subtopic,
        }));

        return subtopic;
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
