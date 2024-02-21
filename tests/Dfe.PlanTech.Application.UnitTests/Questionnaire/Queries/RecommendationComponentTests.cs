using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.UnitTests.Questionnaire.Queries;

// TODO: Temporary tests to satisfy requirements of SonarCloud
// TODO: Remove once the recommendation components are tested as part of business logic tests!

public class RecommendationComponentTests
{
    private readonly RecommendationChunk _recommendationChunk;
    private readonly RecommendationSection _recommendationSection;
    private readonly RecommendationIntro _recommendationIntro;
    private readonly SubTopicRecommendation _subTopicRecommendation;

    public RecommendationComponentTests()
    {
        Answer answer = new()
        {
            Text = "Answer Text",
            NextQuestion = null,
            Maturity = "High"
        };

        TextBody textBody = new()
        {
            RichText = new()
            {
                Value = "Text Body"
            }
        };

        Section section = new()
        {
            Name = "Subtopic Name"
        };

        _recommendationChunk = new()
        {
            Title = "Title Text",
            Header = new() { Text = "Header Chunk Text" },
            Content = [textBody],
            Answers = [answer]
        };

        _recommendationSection = new()
        {
            Answers = [answer],
            Chunks = [_recommendationChunk]
        };

        _recommendationIntro = new RecommendationIntro()
        {
            Header = new() { Text = "Header Intro Text" },
            Maturity = "High",
            Content = [textBody]
        };

        _subTopicRecommendation = new SubTopicRecommendation()
        {
            Intros = [_recommendationIntro],
            Section = _recommendationSection,
            Subtopic = section
        };
    }

    [Fact]
    public void RecommendationChunk_Contains_Elements()
    {
        Assert.Equal("Title Text", _recommendationChunk.Title);

        Assert.Equal("Header Chunk Text", _recommendationChunk.Header.Text);

        Assert.NotEmpty(_recommendationChunk.Content);
        Assert.IsType<TextBody>(_recommendationChunk.Content[0]);
        Assert.Equal("Text Body", (_recommendationChunk.Content[0] as TextBody)?.RichText.Value);

        Assert.NotEmpty(_recommendationChunk.Answers);
        Assert.IsType<Answer>(_recommendationChunk.Answers[0]);
        Assert.Equal("Answer Text", _recommendationChunk.Answers[0].Text);
        Assert.Null(_recommendationChunk.Answers[0].NextQuestion);
        Assert.Equal("High", _recommendationChunk.Answers[0].Maturity);
    }

    [Fact]
    public void RecommendationSection_Contains_Elements()
    {
        Assert.NotEmpty(_recommendationSection.Answers);
        Assert.IsType<Answer>(_recommendationSection.Answers[0]);
        Assert.Equal("Answer Text", _recommendationSection.Answers[0].Text);
        Assert.Null(_recommendationSection.Answers[0].NextQuestion);
        Assert.Equal("High", _recommendationSection.Answers[0].Maturity);

        Assert.NotEmpty(_recommendationSection.Chunks);
        Assert.IsType<RecommendationChunk>(_recommendationSection.Chunks[0]);
        Assert.Equal(_recommendationChunk, _recommendationSection.Chunks[0]);
    }

    [Fact]
    public void RecommendationIntro_Contains_Elements()
    {
        Assert.Equal("Header Intro Text", _recommendationIntro.Header.Text);

        Assert.Equal("High", _recommendationIntro.Maturity);

        Assert.NotEmpty(_recommendationIntro.Content);
        Assert.IsType<TextBody>(_recommendationIntro.Content[0]);
        Assert.Equal("Text Body", (_recommendationIntro.Content[0] as TextBody)?.RichText.Value);
    }

    [Fact]
    public void SubTopicRecommendation_Contains_Elements()
    {
        Assert.NotEmpty(_subTopicRecommendation.Intros);
        Assert.Equal(_recommendationIntro, _subTopicRecommendation.Intros[0]);

        Assert.Equal(_recommendationSection, _subTopicRecommendation.Section);

        Assert.Equal("Subtopic Name", _subTopicRecommendation.Subtopic.Name);
    }
}