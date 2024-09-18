using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.Web.SeedTestData.ContentGenerators;

/// <summary>
/// Creates a category with a topic missing required data in certain draft components
/// All pages should function without error when usePreview = false with this content
/// </summary>
/// <param name="db"></param>
public class MissingDataCategory(CmsDbContext db) : ContentGenerator
{
    private static SectionDbEntity GetSubtopicWithNoInterstitialPage()
    {
        return CreateDraftComponent(new SectionDbEntity
        {
            Name = "Draft Topic with missing Interstitial Page",
            Order = 0,
            Questions =
            [
                CreateComponent(new QuestionDbEntity
                {
                    Slug = "missing-q1",
                    Text = "Missing data question 1",
                    Order = 0,
                    Answers =
                    [
                        CreateComponent(new AnswerDbEntity()
                        {
                            Text = "",
                            Maturity = ""
                        })
                    ]
                })
            ]
        });
    }

    private static SectionDbEntity GetSubtopicWithEmptyInterstitialPage()
    {
        return CreateComponent(new SectionDbEntity
        {
            Name = "Topic with draft Interstitial Page with no content",
            InterstitialPage = CreateDraftComponent(new PageDbEntity
            {
                InternalName = "interstitial-no-content",
                Slug = "interstitial-no-content",
                Title = CreateComponent(new TitleDbEntity { Text = "Interstitial Page with no content" }),
            }),
            Order = 1
        });
    }

    private static SectionDbEntity GetSubtopicWithInterstitialPageWithEmptyTextBody()
    {
        return CreateComponent(new SectionDbEntity
        {
            Name = "Topic with Interstitial Page with empty text body in content",
            InterstitialPage = CreateComponent(new PageDbEntity
            {
                InternalName = "interstitial-empty-content",
                Slug = "interstitial-empty-content",
                Title = CreateComponent(new TitleDbEntity { Text = "Interstitial Page with some empty content" }),
                Content =
                [
                    CreateComponent(new HeaderDbEntity()
                    {
                        Text = "Title for page with empty text body",
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large
                    }),
                    CreateDraftComponent(new TextBodyDbEntity())
                ]
            }),
            Order = 2
        });
    }

    private static SectionDbEntity GetSubtopicWithInterstitialPageWithEmptyWarning()
    {
        return CreateComponent(new SectionDbEntity
        {
            Name = "Topic with Interstitial Page with empty warning",
            InterstitialPage = CreateComponent(new PageDbEntity
            {
                InternalName = "interstitial-empty-warning",
                Slug = "interstitial-empty-warning",
                BeforeTitleContent = [CreateDraftComponent(new WarningComponentDbEntity())],
                Title = CreateComponent(new TitleDbEntity { Text = "Interstitial Page with an empty warning" }),
                Content =
                [
                    CreateComponent(new HeaderDbEntity()
                    {
                        Text = "Title for page with empty warning",
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large
                    })
                ]
            }),
            Order = 3
        });
    }

    private SectionDbEntity GetSubtopicWithPartialRecommendationChunks()
    {
        var answer = CreateComponent(new AnswerDbEntity()
        {
            Text = "Ok",
            Maturity = "High",
        });
        var question = CreateComponent(new QuestionDbEntity()
        {
            Slug = "partial-chunks-q1",
            Text = "This answer includes links to all recommendation chunks",
            Order = 0,
            Answers = [answer]
        });

        var recommendationChunkBlankHeader = CreateDraftComponent(new RecommendationChunkDbEntity()
        {
            Content =
            [
                CreateComponent(new TextBodyDbEntity()
                {
                    RichText = new RichTextContentDbEntity()
                    {
                        Data = new RichTextDataDbEntity() { Uri = "uri" },
                        Marks = [new RichTextMarkDbEntity() { Type = "Bold" }],
                        Value = "rich-text",
                        NodeType = "paragraph"
                    }
                })
            ],
            Answers = [answer],
            Header = ""
        });
        var recommendationChunkNoContent = CreateDraftComponent(new RecommendationChunkDbEntity()
        {
            Header = "Recommendation Chunk with no content",
            Answers = [answer]
        });

        var recommendationSection = CreateComponent(new RecommendationSectionDbEntity()
        {
            Answers = [],
            Chunks = [recommendationChunkBlankHeader, recommendationChunkNoContent]
        });

        var subtopic = CreateComponent(new SectionDbEntity
        {
            Name = "Topic with partial recommendation chunks",
            InterstitialPage = CreateComponent(new PageDbEntity
            {
                InternalName = "topic-partial-chunks",
                Slug = "topic-partial-chunks",
                Title = CreateComponent(new TitleDbEntity { Text = "Partial Recommendation Chunks Topic" }),
                Content =
                [
                    CreateComponent(new ButtonWithEntryReferenceDbEntity
                    {
                        Button = CreateComponent(new ButtonDbEntity { Value = "Continue" }),
                        LinkToEntry = question,
                    })
                ]
            }),
            Order = 4,
            Questions = [question]
        });

        db.SubtopicRecommendations.Add(CreateComponent(new SubtopicRecommendationDbEntity()
        {
            Intros =
            [
                CreateComponent(new RecommendationIntroDbEntity()
                {
                    Slug = "partial-recommendation-intro",
                    Maturity = "High",
                    Header = CreateComponent(new HeaderDbEntity()
                    {
                        Text = "Partial recommendation header",
                        Tag = HeaderTag.H1,
                        Size = HeaderSize.Large
                    })
                })
            ],
            Section = recommendationSection,
            Subtopic = subtopic,
        }));

        return subtopic;
    }

    public override void CreateData()
    {
        List<SectionDbEntity> sections =
        [
            GetSubtopicWithNoInterstitialPage(),
            GetSubtopicWithEmptyInterstitialPage(),
            GetSubtopicWithInterstitialPageWithEmptyTextBody(),
            GetSubtopicWithInterstitialPageWithEmptyWarning(),
            GetSubtopicWithPartialRecommendationChunks()
        ];
        db.Categories.Add(CreateComponent(CreateComponent(new CategoryDbEntity
        {
            InternalName = "Missing Data Category",
            Header = CreateComponent(new HeaderDbEntity
            {
                Text = "Missing Data Category",
                Tag = HeaderTag.H2,
                Size = HeaderSize.Large,
            }),
            Sections = sections,
            Order = 1
        })));
    }
}
