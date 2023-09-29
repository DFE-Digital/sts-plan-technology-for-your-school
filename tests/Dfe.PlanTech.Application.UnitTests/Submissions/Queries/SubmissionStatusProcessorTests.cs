using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interface;
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

  private const string SectionSlug = "section-slug";
  private static readonly ISection _section = new Section()
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

  private readonly ISection[] _sections = new[] { _section };

  public SubmissionStatusProcessorTests()
  {
    _getSectionQuery = Substitute.For<IGetSectionQuery>();
    _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
    _getResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
    _user = Substitute.For<IUser>();
  }

  [Fact]
  public async Task Should_Retrieve_EstablishmentId()
  {
    ISubmissionStatusProcessor processor = new SubmissionStatusProcessor(_getSectionQuery,
                                                                         _getSubmissionStatusesQuery,
                                                                         _statusCheckers,
                                                                         _getResponsesQuery,
                                                                         _user);

    _user.GetEstablishmentId()
         .Returns(1);

    _getSectionQuery.GetSectionBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns(new Section() { });

    _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(Arg.Any<int>(), Arg.Any<Section>(), Arg.Any<CancellationToken>())
                               .Returns(new SectionStatusNew());


    await processor.GetJourneyStatusForSection("section slug", default);

    await _user.Received().GetEstablishmentId();
  }

  [Fact]
  public async Task Should_GetSectionBySlug()
  {
    ISubmissionStatusProcessor processor = new SubmissionStatusProcessor(_getSectionQuery,
                                                                         _getSubmissionStatusesQuery,
                                                                         _statusCheckers,
                                                                         _getResponsesQuery,
                                                                         _user);

    int establishmentId = 1;

    _user.GetEstablishmentId()
         .Returns(establishmentId);

    _getSectionQuery.GetSectionBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns((callinfo) =>
                    {
                      var sectionSlug = callinfo.ArgAt<string>(0);

                      return _sections.FirstOrDefault(section => section.InterstitialPage.Slug == sectionSlug) as Section;
                    });

    _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(Arg.Any<int>(), Arg.Any<Section>(), Arg.Any<CancellationToken>())
                               .Returns(new SectionStatusNew());

    await processor.GetJourneyStatusForSection(SectionSlug, default);

    await _getSubmissionStatusesQuery.Received().GetSectionSubmissionStatusAsync(establishmentId, _section, Arg.Any<CancellationToken>());
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

    ISubmissionStatusProcessor processor = new SubmissionStatusProcessor(_getSectionQuery,
                                                                         _getSubmissionStatusesQuery,
                                                                         new[] { _statusCheckers[0], successStatusChecker },
                                                                         _getResponsesQuery,
                                                                         _user);

    int establishmentId = 1;

    _user.GetEstablishmentId()
         .Returns(establishmentId);

    _getSectionQuery.GetSectionBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns((callinfo) =>
                    {
                      var sectionSlug = callinfo.ArgAt<string>(0);

                      return _sections.FirstOrDefault(section => section.InterstitialPage.Slug == sectionSlug) as Section;
                    });

    _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(Arg.Any<int>(), Arg.Any<Section>(), Arg.Any<CancellationToken>())
                               .Returns(new SectionStatusNew());

    await processor.GetJourneyStatusForSection(SectionSlug, default);

    _failureStatusChecker.Received(1).IsMatchingSubmissionStatus(processor);
    await _failureStatusChecker.DidNotReceive().ProcessSubmission(processor, Arg.Any<CancellationToken>());

    successStatusChecker.Received(1).IsMatchingSubmissionStatus(processor);
    await successStatusChecker.Received(1).ProcessSubmission(processor, Arg.Any<CancellationToken>());
  }
}