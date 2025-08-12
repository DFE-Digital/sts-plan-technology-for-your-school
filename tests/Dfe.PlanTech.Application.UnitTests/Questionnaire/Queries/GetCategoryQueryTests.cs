using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Questionnaire.Queries;

public class GetCategoryQueryTests
{
    private readonly static Section _firstSection = new()
    {
        Name = "First section",
        InterstitialPage = new Page()
        {
            Slug = "first"
        },
        Sys = new SystemDetails()
        {
            Id = "1"
        }
    };

    private readonly static Section _secondSection = new()
    {
        Name = "Second section",
        InterstitialPage = new Page()
        {
            Slug = "second"
        },
        Sys = new SystemDetails()
        {
            Id = "2"
        }
    };

    private readonly static Category _category = new()
    {
        LandingPage = new Page()
        {
            Slug = "category-one"
        },
        Sections = new List<Section> { _firstSection, _secondSection }
    };

    private readonly static Category _categoryTwo = new()
    {
        LandingPage = new Page()
        {
            Slug = "category-two",
        },
        Sections = new List<Section>() { _firstSection, _secondSection }
    };

    private readonly Category[] _categories = new[] { _category, _categoryTwo };

    [Fact]
    public async Task GetCategoryBySlug_Returns_Category_From_Repository()
    {
        Assert.NotNull(_category.LandingPage);
        var categorySlug = _category.LandingPage.Slug;
        var cancellationToken = CancellationToken.None;

        var repository = Substitute.For<IContentRepository>();

        repository.GetEntities<Category>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
        .Returns(callInfo =>
        {
            var options = callInfo.ArgAt<GetEntitiesOptions>(0);

            var slugQuery = (options.Queries?.FirstOrDefault(query =>
                query is ContentQueryEquals equalsQuery &&
                equalsQuery.Field == GetCategoryQuery.SlugFieldPath) as ContentQueryEquals)
                ?? throw new InvalidOperationException("Missing query for slug");

            return _categories.Where(category => category.LandingPage?.Slug == slugQuery.Value);
        });


        var getCategoryQuery = new GetCategoryQuery(repository);
        await getCategoryQuery.GetCategoryBySlug(categorySlug, cancellationToken);

        Assert.Equal(_category.LandingPage.Slug, categorySlug);

        await repository.Received(1).GetEntities<Category>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCategoryBySlug_ThrowsExceptionOnRepositoryError()
    {
        var categorySlug = "test-category";
        var cancellationToken = CancellationToken.None;

        var repository = Substitute.For<IContentRepository>();
        repository
            .When(repo => repo.GetEntities<Category>(Arg.Any<GetEntitiesOptions>(), cancellationToken))
            .Throw(new Exception("Dummy Exception"));

        var getCategoryQuery = new GetCategoryQuery(repository);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
            async () => await getCategoryQuery.GetCategoryBySlug(categorySlug, cancellationToken)
        );
    }
}

