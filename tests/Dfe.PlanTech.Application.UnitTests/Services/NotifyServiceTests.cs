using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class NotifyServiceTests
{
    private readonly INotifyWorkflow _notifyWorkflow = Substitute.For<INotifyWorkflow>();

    private static SystemDetails BuildSystemDetails(string id) => new SystemDetails { Id = id };

    [Fact]
    public void Constructor_WhenNotifyWorkflowIsNull_ThrowsArgumentNullException()
    {
        var act = () => new NotifyService(null!);

        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("notifyWorkflow", exception.ParamName);
    }

    [Fact]
    public void SendSingleRecommendationEmail_WhenUserMessageIsNull_SendsExpectedPersonalisation()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { NameOfUser = "Drew", UserMessage = null };

        var textBody = new ComponentTextBodyEntry { RichText = CreateMinimalRichTextDocument() };

        var expectedResults = new List<NotifySendResult>();

        _notifyWorkflow
            .SendEmails(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<Dictionary<string, object>>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            )
            .Returns(expectedResults);

        var result = sut.SendSingleRecommendationEmail(
            model,
            textBody,
            "Test School",
            "Test Recommendation",
            "Digital leadership",
            RecommendationStatus.Complete
        );

        Assert.Same(expectedResults, result);

        _notifyWorkflow
            .Received(1)
            .SendEmails(
                model,
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["name of user"] == "Drew"
                    && (string)d["school"] == "Test School"
                    && (string)d["standard"] == "digital leadership"
                    && (string)d["user message"] == string.Empty
                    && (string)d["recommendation name"] == "Test Recommendation"
                    && (string)d["status"] == RecommendationStatus.Complete.GetDisplayName()
                    && d.ContainsKey("recommendation")
                ),
                Arg.Any<string>(),
                NotifyConstants.ShareSingleRecommendationTemplateId
            );
    }

    [Fact]
    public void SendSingleRecommendationEmail_WhenUserMessageHasSingleLine_PrefixesAndQuotesMessage()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { NameOfUser = "Drew", UserMessage = "Hello team" };

        var textBody = new ComponentTextBodyEntry { RichText = CreateMinimalRichTextDocument() };

        sut.SendSingleRecommendationEmail(
            model,
            textBody,
            "Test School",
            "Test Recommendation",
            "Digital leadership",
            RecommendationStatus.InProgress
        );

        var expectedMessage =
            "Drew added a message:" + Environment.NewLine + Environment.NewLine + "^ Hello team";

        _notifyWorkflow
            .Received(1)
            .SendEmails(
                model,
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["user message"] == expectedMessage
                ),
                Arg.Any<string>(),
                NotifyConstants.ShareSingleRecommendationTemplateId
            );
    }

    [Fact]
    public void SendSingleRecommendationEmail_WhenUserMessageHasMultipleLines_PrefixesAndQuotesEachLine()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel
        {
            NameOfUser = "Drew",
            UserMessage = "Line one\r\nLine two\nLine three",
        };

        var textBody = new ComponentTextBodyEntry { RichText = CreateMinimalRichTextDocument() };

        sut.SendSingleRecommendationEmail(
            model,
            textBody,
            "Test School",
            "Test Recommendation",
            "Digital leadership",
            RecommendationStatus.NotStarted
        );

        var expectedMessage = string.Join(
            Environment.NewLine,
            ["Drew added a message:", string.Empty, "^ Line one", "^ Line two", "^ Line three"]
        );

        _notifyWorkflow
            .Received(1)
            .SendEmails(
                model,
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["user message"] == expectedMessage
                ),
                Arg.Any<string>(),
                NotifyConstants.ShareSingleRecommendationTemplateId
            );
    }

    [Fact]
    public void SendStandardEmail_WhenUserMessageIsWhitespace_UsesEmptyMessageAndBuildsRecommendationsMarkdown()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { NameOfUser = "Drew", UserMessage = "   " };

        var sections = new List<QuestionnaireSectionEntry>
        {
            new()
            {
                Sys = BuildSystemDetails("section-1"),
                Name = "Digital technology registers",
                CoreRecommendations =
                [
                    new RecommendationChunkEntry
                    {
                        Sys = BuildSystemDetails("rec-1"),
                        Header = "Assign a digital lead",
                    },
                ],
            },
        };

        var sectionStatuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "section-1", LastCompletionDate = new DateTime(2026, 3, 1) },
        };

        var recommendationStatuses = new Dictionary<
            string,
            SqlEstablishmentRecommendationHistoryDto
        >
        {
            ["rec-1"] = new() { NewStatus = RecommendationStatus.Complete },
        };

        var expectedResults = new List<NotifySendResult>();

        _notifyWorkflow
            .SendEmails(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<Dictionary<string, object>>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            )
            .Returns(expectedResults);

        var result = sut.SendStandardEmail(
            model,
            sections,
            sectionStatuses,
            recommendationStatuses,
            "Digital Standards",
            "Test School"
        );

        Assert.Same(expectedResults, result);

        _notifyWorkflow
            .Received(1)
            .SendEmails(
                model,
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["name of user"] == "Drew"
                    && (string)d["school"] == "Test School"
                    && (string)d["standard lowercase"] == "digital standards"
                    && (string)d["user message"] == string.Empty
                    && (string)d["standard"] == "Digital Standards"
                    && ((string)d["recommendations"]).Contains(
                        "## Recommendations for digital technology registers"
                    )
                    && ((string)d["recommendations"]).Contains(
                        "The self-assessment for digital technology registers was completed on 01 March 2026."
                    )
                    && ((string)d["recommendations"]).Contains(
                        $"1. {RecommendationStatus.Complete.GetDisplayName()}: Assign a digital lead"
                    )
                ),
                Arg.Any<string>(),
                NotifyConstants.ShareStandardTemplateId
            );
    }

    [Fact]
    public void SendStandardEmail_WhenUserMessageHasContent_PrefixesAndQuotesMessage()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel
        {
            NameOfUser = "Drew",
            UserMessage = "Please take a look",
        };

        var sections = new List<QuestionnaireSectionEntry>
        {
            new()
            {
                Sys = BuildSystemDetails("section-1"),
                Name = "Digital technology registers",
                CoreRecommendations = [],
            },
        };

        var sectionStatuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "section-1", LastCompletionDate = null },
        };

        var recommendationStatuses =
            new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>();

        sut.SendStandardEmail(
            model,
            sections,
            sectionStatuses,
            recommendationStatuses,
            "Digital Standards",
            "Test School"
        );

        var expectedMessage =
            "Drew added a message:"
            + Environment.NewLine
            + Environment.NewLine
            + "^ Please take a look";

        _notifyWorkflow
            .Received(1)
            .SendEmails(
                model,
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["user message"] == expectedMessage
                ),
                Arg.Any<string>(),
                NotifyConstants.ShareStandardTemplateId
            );
    }

    [Fact]
    public void SendStandardEmail_WhenSectionNotCompleted_BuildsNotCompletedMarkdown()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { NameOfUser = "Drew" };

        var sections = new List<QuestionnaireSectionEntry>
        {
            new()
            {
                Sys = BuildSystemDetails("section-1"),
                Name = "Digital technology registers",
                CoreRecommendations =
                [
                    new RecommendationChunkEntry
                    {
                        Sys = BuildSystemDetails("rec-1"),
                        Header = "Assign a digital lead",
                    },
                ],
            },
        };

        var sectionStatuses = new List<SqlSectionStatusDto>();
        var recommendationStatuses =
            new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>();

        sut.SendStandardEmail(
            model,
            sections,
            sectionStatuses,
            recommendationStatuses,
            "Digital Standards",
            "Test School"
        );

        _notifyWorkflow
            .Received(1)
            .SendEmails(
                model,
                Arg.Is<Dictionary<string, object>>(d =>
                    ((string)d["recommendations"]).Contains(
                        "## Recommendations for digital technology registers"
                    )
                    && ((string)d["recommendations"]).Contains(
                        "The self-assessment hasn't been completed."
                    )
                    && !((string)d["recommendations"]).Contains("1.")
                ),
                Arg.Any<string>(),
                NotifyConstants.ShareStandardTemplateId
            );
    }

    [Fact]
    public void SendStandardEmail_WhenRecommendationStatusMissing_ThrowsInvalidOperationException()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { NameOfUser = "Drew" };

        var sections = new List<QuestionnaireSectionEntry>
        {
            new()
            {
                Sys = BuildSystemDetails("section-1"),
                Name = "Digital technology registers",
                CoreRecommendations =
                [
                    new RecommendationChunkEntry
                    {
                        Sys = BuildSystemDetails("rec-1"),
                        Header = "Assign a digital lead",
                    },
                ],
            },
        };

        var sectionStatuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "section-1", LastCompletionDate = new DateTime(2026, 3, 1) },
        };

        var recommendationStatuses =
            new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>();

        var act = () =>
            sut.SendStandardEmail(
                model,
                sections,
                sectionStatuses,
                recommendationStatuses,
                "Digital Standards",
                "Test School"
            );

        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal(
            "Cannot prepare markdown for a recommendation that does not exist in the database.",
            exception.Message
        );
    }

    [Fact]
    public void SendStandardEmail_WhenRecommendationStatusExistsButNewStatusIsNull_UsesNotStarted()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { NameOfUser = "Drew" };

        var sections = new List<QuestionnaireSectionEntry>
        {
            new()
            {
                Sys = BuildSystemDetails("section-1"),
                Name = "Digital technology registers",
                CoreRecommendations =
                [
                    new RecommendationChunkEntry
                    {
                        Sys = BuildSystemDetails("rec-1"),
                        Header = "Assign a digital lead",
                    },
                ],
            },
        };

        var sectionStatuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = "section-1", LastCompletionDate = new DateTime(2026, 3, 1) },
        };

        var recommendationStatuses = new Dictionary<
            string,
            SqlEstablishmentRecommendationHistoryDto
        >
        {
            ["rec-1"] = new() { NewStatus = null },
        };

        sut.SendStandardEmail(
            model,
            sections,
            sectionStatuses,
            recommendationStatuses,
            "Digital Standards",
            "Test School"
        );

        _notifyWorkflow
            .Received(1)
            .SendEmails(
                model,
                Arg.Is<Dictionary<string, object>>(d =>
                    ((string)d["recommendations"]).Contains(
                        $"1. {RecommendationStatus.NotStarted.GetDisplayName()}: Assign a digital lead"
                    )
                ),
                Arg.Any<string>(),
                NotifyConstants.ShareStandardTemplateId
            );
    }

    private NotifyService CreateSut() => new(_notifyWorkflow);

    private static RichTextContentField CreateMinimalRichTextDocument()
    {
        return new RichTextContentField
        {
            NodeType = "document",
            Content =
            [
                new RichTextContentField
                {
                    NodeType = "paragraph",
                    Content =
                    [
                        new RichTextContentField
                        {
                            NodeType = "text",
                            Value = "Rendered recommendation body",
                            Marks = [],
                            Data = new(),
                        },
                    ],
                    Data = new(),
                },
            ],
            Data = new(),
        };
    }
}
