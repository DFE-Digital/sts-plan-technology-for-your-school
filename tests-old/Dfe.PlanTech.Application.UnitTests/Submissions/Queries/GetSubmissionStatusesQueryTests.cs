using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class GetSubmissionStatusesQueryTests
{
    private readonly IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();
    private readonly IUser user = Substitute.For<IUser>();

    private readonly List<SectionStatusDto> SectionStatuses = new List<SectionStatusDto>
    {
        new SectionStatusDto
        {
            Completed = true,
            SectionId = "1",
            LastMaturity = "Low",
            DateCreated = DateTime.UtcNow,
        },
        new SectionStatusDto
        {
            Completed = true,
            SectionId = "2",
            LastMaturity = "High",
            DateCreated = DateTime.UtcNow,
        },
        new SectionStatusDto
        {
            Completed = false,
            SectionId = "3",
            DateCreated = DateTime.UtcNow,
        },
        new SectionStatusDto
        {
            Completed = false,
            SectionId = "4",
            DateCreated = DateTime.UtcNow,
        },
    };

    private const int establishmentId = 1;
    private const string maturity = "High";

    private static readonly Section completeSection = new()
    {
        Sys = new SystemDetails() { Id = "1" },
        Name = "section one",
    };

    private static readonly Section inprogressSection = new()
    {
        Sys = new SystemDetails() { Id = "2" },
        Name = "section two",
    };

    private static readonly Section notstartedSection = new()
    {
        Sys = new SystemDetails() { Id = "3" },
        Name = "section three",
    };

    private readonly Submission[] submissions = new Submission[]
    {
        new()
        {
            SectionId = completeSection.Sys.Id,
            EstablishmentId = 1,
            SectionName = completeSection.Name,
            Completed = true,
            DateCreated = DateTime.UtcNow.AddDays(-7),
            Maturity = maturity,
            Responses = new List<Response>() { new() { Id = 1 } },
        },
        new()
        {
            SectionId = completeSection.Sys.Id,
            EstablishmentId = 2,
            SectionName = completeSection.Name,
            Completed = false,
        },
        new()
        {
            SectionId = inprogressSection.Sys.Id,
            EstablishmentId = 1,
            SectionName = inprogressSection.Name,
            Completed = false,
            Responses = new List<Response>() { new() { Id = 2 } },
        },
        new()
        {
            SectionId = inprogressSection.Sys.Id,
            EstablishmentId = 1,
            SectionName = inprogressSection.Name,
            Completed = false,
        },
    };

    private readonly Category[] categories = new Category[]
    {
        new()
        {
            Sys = new SystemDetails() { Id = "A" },
            Sections = new List<Section>() { completeSection, notstartedSection },
        },
        new()
        {
            Sys = new SystemDetails() { Id = "B" },
            Sections = new List<Section>() { inprogressSection },
        },
    };

    private GetSubmissionStatusesQuery CreateStrut() => new GetSubmissionStatusesQuery(Db, user);

    public GetSubmissionStatusesQueryTests()
    {
        Db.GetSectionStatuses(Arg.Any<string>(), Arg.Any<int>())
            .Returns(
                (callinfo) =>
                {
                    var sectionIds = callinfo.ArgAt<string>(0).Split(",");
                    return SectionStatuses
                        .Where(sectionStatus => sectionIds.Contains(sectionStatus.SectionId))
                        .AsQueryable();
                }
            );

        Db.GetSubmissions.Returns(submissions.AsQueryable());

        Db.FirstOrDefaultAsync(Arg.Any<IQueryable<Submission>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var queryable = callInfo.ArgAt<IQueryable<Submission>>(0);
                var first = queryable.FirstOrDefault();
                return Task.FromResult(first);
            });

        Db.ToListAsync(Arg.Any<IQueryable<SectionStatusDto>>(), Arg.Any<CancellationToken>())
            .Returns(
                (callinfo) =>
                {
                    var query = callinfo.ArgAt<IQueryable<SectionStatusDto>>(0);

                    return query.ToList();
                }
            );

        user.GetEstablishmentId().Returns(establishmentId);
    }

    [Fact]
    public async Task GetSectionSubmissionStatuses_ReturnsListOfStatuses()
    {
        var category = categories[0];
        var sections = category.Sections;
        var result = await CreateStrut().GetSectionSubmissionStatuses(category.Sections);

        Assert.Equal(result.Count, sections.Count);

        foreach (var section in sections)
        {
            var matchingSectionStatus = result.FirstOrDefault(sectionStatus =>
                sectionStatus.SectionId == section.Sys.Id
            );

            Assert.NotNull(matchingSectionStatus);
        }
    }

    [Fact]
    public async Task GetSectionSubmissionStatusAsync_Returns_Status_Completed_When_Found_With_Responses()
    {
        var result = await CreateStrut()
            .GetSectionSubmissionStatusAsync(establishmentId, completeSection, true, default);

        Assert.NotNull(result);

        Assert.Equal(completeSection.Sys.Id, result.SectionId);
        Assert.Equal(Status.CompleteReviewed, result.Status);
        Assert.Equal(maturity, result.Maturity);
        Assert.True(result.Completed);
    }

    [Fact]
    public async Task GetSectionSubmissionStatusAsync_Returns_Status_InProgress_When_Found_Incomplete_With_Responses()
    {
        var result = await CreateStrut()
            .GetSectionSubmissionStatusAsync(establishmentId, inprogressSection, false, default);

        Assert.NotNull(result);

        Assert.Equal(inprogressSection.Sys.Id, result.SectionId);
        Assert.Equal(Status.InProgress, result.Status);
        Assert.Null(result.Maturity);
        Assert.False(result.Completed);
    }

    [Fact]
    public async Task GetSectionSubmissionStatusAsync_Returns_Status_NotStarted_When_Found_With_No_Responses()
    {
        var result = await CreateStrut()
            .GetSectionSubmissionStatusAsync(establishmentId, notstartedSection, false, default);

        Assert.NotNull(result);

        Assert.Equal(notstartedSection.Sys.Id, result.SectionId);
        Assert.Equal(Status.NotStarted, result.Status);
        Assert.Null(result.Maturity);
        Assert.False(result.Completed);
    }

    [Fact]
    public async Task GetSectionSubmissionStatusAsync_Returns_Status_NotStarted_When_Not_Found()
    {
        var section = new Section()
        {
            Sys = new SystemDetails() { Id = "not started at all section" },
        };

        var result = await CreateStrut()
            .GetSectionSubmissionStatusAsync(establishmentId, section, false, default);

        Assert.NotNull(result);

        Assert.Equal(section.Sys.Id, result.SectionId);
        Assert.Equal(Status.NotStarted, result.Status);
        Assert.Null(result.Maturity);
        Assert.False(result.Completed);
    }
}
