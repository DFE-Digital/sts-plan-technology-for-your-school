using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Routing;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class SubmissionStatusProcessorTests
{
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
    private readonly IGetLatestResponsesQuery _getResponsesQuery;
    private readonly IUser _user;

    private static readonly ISubmissionStatusChecker _failureStatusChecker = Substitute.For<ISubmissionStatusChecker>();
    private readonly ISubmissionStatusChecker[] _statusCheckers = new[] { _failureStatusChecker };

    private const int _establishmentId = 1;

    private const string SectionSlug = "section-slug";
    private static readonly Section _section = new Section()
    {
        Sys = new SystemDetails()
        {
            Id = "section-id"
        },
        InterstitialPage = new Page()
        {
            Slug = SectionSlug
        }
    };

    private readonly Section[] _sections = new[] { _section };

    public SubmissionStatusProcessorTests()
    {
        _getSectionQuery = Substitute.For<IGetSectionQuery>();
        _getSectionQuery.GetSectionBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns((callinfo) =>
                        {
                            var sectionSlug = callinfo.ArgAt<string>(0);

                            return _sections.FirstOrDefault(section => section.InterstitialPage.Slug == sectionSlug);
                        });

        _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
        _getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();

        _user = Substitute.For<IUser>();
        _user.GetEstablishmentId().Returns(_establishmentId);
    }

    [Fact]
    public async Task Should_Use_StatusCheckers()
    {
        ISubmissionStatusChecker successStatusChecker = Substitute.For<ISubmissionStatusChecker>();
        successStatusChecker.IsMatchingSubmissionStatus(Arg.Any<SubmissionStatusProcessor>())
                      .Returns((callinfo) =>
                      {
                          var processor = callinfo.ArgAt<SubmissionStatusProcessor>(0);

                          return processor.Section == _section;
                      });

        _failureStatusChecker.IsMatchingSubmissionStatus(Arg.Any<SubmissionStatusProcessor>()).Returns(false);

        SubmissionStatusProcessor processor = new SubmissionStatusProcessor(_getSectionQuery,
                                                                             _getSubmissionStatusesQuery,
                                                                             new[] { _statusCheckers[0], successStatusChecker },
                                                                             _getResponsesQuery,
                                                                             _user);

        _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(Arg.Any<int>(), Arg.Any<Section>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                                   .Returns(new SectionStatusNew());

        await processor.GetJourneyStatusForSection(SectionSlug, default);

        _failureStatusChecker.Received(1).IsMatchingSubmissionStatus(processor);
        await _failureStatusChecker.DidNotReceive().ProcessSubmission(processor, Arg.Any<CancellationToken>());

        successStatusChecker.Received(1).IsMatchingSubmissionStatus(processor);
        await successStatusChecker.Received(1).ProcessSubmission(processor, Arg.Any<CancellationToken>());
    }



    [Fact]
    public async Task Should_Throw_Exception_When_NoStatusChecker_Matches()
    {
        _failureStatusChecker.IsMatchingSubmissionStatus(Arg.Any<SubmissionStatusProcessor>()).Returns(false);

        SubmissionStatusProcessor processor = new SubmissionStatusProcessor(_getSectionQuery,
                                                                             _getSubmissionStatusesQuery,
                                                                             _statusCheckers,
                                                                             _getResponsesQuery,
                                                                             _user);

        _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(Arg.Any<int>(), Arg.Any<Section>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                                   .Returns(new SectionStatusNew());

        await Assert.ThrowsAnyAsync<InvalidDataException>(() => processor.GetJourneyStatusForSection(SectionSlug, default));
    }

    [Fact]
    public async Task Should_ThrowException_When_Section_NotFound()
    {
        SubmissionStatusProcessor processor = new SubmissionStatusProcessor(_getSectionQuery,
                                                                             _getSubmissionStatusesQuery,
                                                                             _statusCheckers,
                                                                             _getResponsesQuery,
                                                                             _user);
        _user.GetEstablishmentId()
             .Returns(1);

        _getSectionQuery.GetSectionBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(null as Section);

        await Assert.ThrowsAnyAsync<ContentfulDataUnavailableException>(() => processor.GetJourneyStatusForSection("not matching section slug", default));
    }
}