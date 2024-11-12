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
    private readonly ICmsCache _cache = Substitute.For<ICmsCache>();

    public GetSectionQueryTests()
    {
        _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<Section>>>>())
            .Returns(callInfo =>
            {
                var func = callInfo.ArgAt<Func<Task<IEnumerable<Section>>>>(1);
                return func();
            });
    }

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

        var getSectionQuery = new GetSectionQuery(repository, _cache);
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

        var getSectionQuery = new GetSectionQuery(repository, _cache);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
            async () => await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken)
        );
    }
}

