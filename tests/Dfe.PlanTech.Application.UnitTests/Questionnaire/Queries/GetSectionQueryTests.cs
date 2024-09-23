
using AutoMapper;
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

    private readonly List<SectionDbEntity> _dbSections = new(3);

    private readonly Section[] _sections = new[] { FirstSection, SecondSection, ThirdSection };

    private readonly IMapper _mapper = Substitute.For<IMapper>();

    public GetSectionQueryTests()
    {
        _dbSections.AddRange(_sections.Select((section) =>
        {
            var interstitialPageId = section.InterstitialPage != null
                ? $"{section.InterstitialPage.Slug}-ID"
                : throw new InvalidOperationException("InterstitialPage cannot be null");

            return new SectionDbEntity()
            {
                Name = section.Name,
                InterstitialPage = new PageDbEntity()
                {
                    Slug = section.InterstitialPage.Slug,
                    Id = interstitialPageId
                },
                InterstitialPageId = interstitialPageId,
                Id = section.Sys.Id
            };
        }));

        _mapper.Map<Section>(Arg.Any<SectionDbEntity>())
                .Returns(callinfo =>
                {
                    var section = callinfo.ArgAt<SectionDbEntity>(0);

                    return new Section()
                    {
                        Name = section.Name,
                        Sys = new()
                        {
                            Id = section.Id
                        }
                    };
                });
    }

    [Fact]
    public async Task GetSectionBySlug_Returns_Section_From_Db()
    {
        var sectionSlug = FirstSection.InterstitialPage?.Slug ?? throw new InvalidOperationException("InterstitialPage cannot be null");
        var cancellationToken = CancellationToken.None;

        var repository = Substitute.For<IContentRepository>();
        repository.GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>())
        .Returns((callinfo) =>
        {
            var options = callinfo.ArgAt<GetEntitiesOptions>(0);

            if (options.Queries == null)
            {
                throw new InvalidOperationException("Queries cannot be null");
            }

            var slugQuery = options.Queries.FirstOrDefault(query => query is ContentQueryEquals equalsQuery &&
                                                            equalsQuery.Field == GetSectionQuery.SlugFieldPath) as ContentQueryEquals ?? throw new InvalidOperationException("Slug query cannot be null");

            return _sections.Where(section => section.InterstitialPage?.Slug == slugQuery.Value);
        });

        var db = Substitute.For<ICmsDbContext>();
        db.Sections.Returns(_dbSections.AsQueryable());
        db.FirstOrDefaultAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>())
                .Returns(callinfo =>
                {
                    var queryable = callinfo.ArgAt<IQueryable<SectionDbEntity>>(0);

                    return queryable.FirstOrDefault();
                });

        var getSectionQuery = new GetSectionQuery(db, repository, _mapper);
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken);

        Assert.NotNull(section); // Add this line to check for null
        Assert.Equal(FirstSection.InterstitialPage.Slug, sectionSlug);

        await db.Received(1).FirstOrDefaultAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>());
        await repository.Received(0).GetEntities<Section>(Arg.Any<GetEntitiesOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSectionBySlug_Returns_Section_From_Repository_When_Db_Returns_Nothing()
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

        var db = Substitute.For<ICmsDbContext>();
        var nullList = new List<SectionDbEntity>();
        db.Sections.Returns(nullList.AsQueryable());
        db.FirstOrDefaultAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>())
                .Returns(callinfo =>
                {
                    var queryable = callinfo.ArgAt<IQueryable<SectionDbEntity>>(0);

                    return queryable.FirstOrDefault();
                });

        var getSectionQuery = new GetSectionQuery(db, repository, _mapper);
        var section = await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken);

        Assert.Equal(FirstSection.InterstitialPage.Slug, sectionSlug);

        await db.Received(1).FirstOrDefaultAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>());
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

        var db = Substitute.For<ICmsDbContext>();

        var mapper = Substitute.For<IMapper>();

        var getSectionQuery = new GetSectionQuery(db, repository, mapper);

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(
            async () => await getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken)
        );
    }
}

