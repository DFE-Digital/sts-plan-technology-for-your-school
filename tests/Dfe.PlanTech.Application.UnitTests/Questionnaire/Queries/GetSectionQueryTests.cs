using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Questionnaire.Queries;

public class GetSectionQueryTests
{
    private readonly static Answer FirstAnswer = new()
    {
        Text = "Answer1",
        Sys = new SystemDetails() { Id = "Answer-1" },
        NextQuestion = new Question()
        {
            Text = "Question 2",
            Sys = new SystemDetails() { Id = "Question-2" },
            Answers = new List<Answer>()
            {

            }
        }
    };
    private readonly static Section FirstSection = new()
    {
        Name = "First section",
        InterstitialPage = new Page()
        {
            Slug = "/first"
        },
        Sys = new SystemDetails()
        {
            Id = "1"
        },
        Questions = new List<Question>()
        {
            new Question()
            {
                Text = "Question 1",
                Sys = new SystemDetails() { Id = "Question-1" },
                Answers = new List<Answer>() { FirstAnswer }
            }
        }
    };

    private readonly static Section SecondSection = new()
    {
        Name = "Second section",
        InterstitialPage = new Page()
        {
            Slug = "/second"
        },
        Sys = new SystemDetails()
        {
            Id = "2"
        }
    };

    private readonly static Section ThirdSection = new()
    {
        Name = "Thurd section",
        InterstitialPage = new Page()
        {
            Slug = "/third"
        },
        Sys = new SystemDetails()
        {
            Id = "3"
        }
    };

    private readonly Section[] _sections = new[] { FirstSection, SecondSection, ThirdSection };

    [Fact]
    public async Task GetSectionBySlug_Returns_Section_From_Repository()
    {
        Assert.NotNull(FirstSection.InterstitialPage);
        var sectionSlug = FirstSection.InterstitialPage.Slug;
        var cancellationToken = CancellationToken.None;

        var repository = Substitute.For<IContentRepository>();
        repository.GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
        .Returns((callinfo) =>
        {
            var options = callinfo.ArgAt<GetEntitiesOptions>(0);

            var slugQuery = (options.Queries?.FirstOrDefault(query => query is ContentQueryEquals equalsQuery &&
                                                            equalsQuery.Field == GetSectionQuery.SlugFieldPath) as ContentQueryEquals) ??
                                            throw new InvalidOperationException("Missing query for slug");


            return _sections.Where(section => section.InterstitialPage?.Slug == slugQuery.Value);
        });

        var getSectionQuery = new GetSectionQuery(repository);
        await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken);

        Assert.Equal(FirstSection.InterstitialPage.Slug, sectionSlug);

        await repository.Received(1).GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>());
    }



    [Fact]
    public async Task GetSectionBySlug_ThrowsExceptionOnRepositoryError()
    {
        var sectionSlug = "section-slug";
        var cancellationToken = CancellationToken.None;

        var repository = Substitute.For<IContentRepository>();
        repository
            .When(repo => repo.GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), cancellationToken))
            .Throw(new Exception("Dummy Exception"));

        var getSectionQuery = new GetSectionQuery(repository);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
            async () => await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken)
        );
    }

    [Fact]
    public async Task GetAllSections_ThrowsExceptionOnRepositoryError()
    {
        var repository = Substitute.For<IContentRepository>();
        repository
            .When(repo => repo.GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>()))
            .Throw(new Exception("Dummy Exception"));

        var getSectionQuery = new GetSectionQuery(repository);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
            async () => await getSectionQuery.GetAllSections(CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetAllSections_Should_Omit_NextQuestion_Answers()
    {
        var repository = Substitute.For<IContentRepository>();
        repository.GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
            .Returns(_ => _sections);

        var getSectionQuery = new GetSectionQuery(repository);
        var sections = (await getSectionQuery.GetAllSections()).ToList();
        Assert.Equal(_sections.Length, sections.Count);

        var nextQuestions = sections
            .SelectMany(section => section!.Questions)
            .SelectMany(question => question.Answers)
            .Where(answer => answer.NextQuestion != null)
            .Select(answer => answer.NextQuestion!)
            .ToList();

        Assert.All(nextQuestions, nextQuestion =>
        {
            Assert.NotNull(nextQuestion.Sys);
            Assert.Empty(nextQuestion.Answers);
        });
    }
}

