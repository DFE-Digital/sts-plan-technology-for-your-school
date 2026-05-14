using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Responses.Queries;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Routing
{
    public class ChangeAnswersRouterTests
    {
        private readonly IGetPageQuery _getPageQuery = Substitute.For<IGetPageQuery>();
        private readonly IProcessSubmissionResponsesCommand _processCommand =
            Substitute.For<IProcessSubmissionResponsesCommand>();
        private readonly IUser _user = Substitute.For<IUser>();
        private readonly ISubmissionStatusProcessor _statusProcessor =
            Substitute.For<ISubmissionStatusProcessor>();
        private readonly ISubmissionCommand _submissionCommand =
            Substitute.For<ISubmissionCommand>();
        private readonly IPlanTechDbContext _dbContext = Substitute.For<IPlanTechDbContext>();
        private IGetLatestResponsesQuery _responsesQuery;

        private readonly ChangeAnswersController _controller = new(
            new NullLogger<ChangeAnswersController>(),
            Substitute.For<IUser>()
        )
        {
            ControllerContext = ControllerHelpers.SubstituteControllerContext(),
        };

        private readonly ChangeAnswersRouter _router;

        private readonly string _categorySlug = "category-slug";
        private readonly string _sectionSlug = "section-slug";
        private readonly int _establishmentId = 1;
        private readonly Section _section;
        private readonly SubmissionResponsesDto _responsesDto;

        private readonly Submission InProgressSubmission = new()
        {
            Id = 1,
            SectionId = "section-id",
            SectionName = "Test Section",
            EstablishmentId = 100,
            Status = SubmissionStatus.InProgress.ToString(),
            Responses = new List<Response>(),
        };

        private readonly Submission LatestCompletedSubmission = new()
        {
            Id = 2,
            SectionId = "section-id",
            SectionName = "Test Section",
            EstablishmentId = 100,
            Status = SubmissionStatus.CompleteReviewed.ToString(),
            Completed = true,
            DateCreated = DateTime.UtcNow.AddDays(-1),
            Responses = new List<Response>(),
        };

        private readonly Submission ClonedSubmission = new()
        {
            Id = 3,
            SectionId = "section-id",
            SectionName = "Test Section",
            EstablishmentId = 100,
            Status = SubmissionStatus.InProgress.ToString(),
            DateCreated = DateTime.UtcNow,
        };

        public ChangeAnswersRouterTests()
        {
            _router = new ChangeAnswersRouter(
                _getPageQuery,
                _processCommand,
                _user,
                _statusProcessor,
                _responsesQuery!,
                _submissionCommand
            );
            _responsesQuery = new GetLatestResponsesQuery(_dbContext);

            _section = new Section
            {
                Name = "Test Section",
                Sys = new() { Id = "section-id" },
                Questions = new List<Question> { new Question { Slug = "q1" } },
            };

            _responsesDto = new SubmissionResponsesDto
            {
                SubmissionId = 123,
                Responses = new List<QuestionWithAnswer>
                {
                    new QuestionWithAnswer { QuestionSysId = "q1", AnswerSysId = "a1" },
                },
            };

            _statusProcessor.Section.Returns(_section);
            _user.GetEstablishmentId().Returns(_establishmentId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ValidateRoute_ShouldThrow_WhenCategorySlugIsNullOrEmpty(
            string? categorySlug
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _router.ValidateRoute(categorySlug!, _sectionSlug, null, _controller, default)
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ValidateRoute_ShouldThrow_WhenSectionSlugIsNullOrEmpty(
            string? sectionSlug
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _router.ValidateRoute(_categorySlug, sectionSlug!, null, _controller, default)
            );
        }

        [Fact]
        public async Task ValidateRoute_ShouldReturnChangeAnswersView_WhenStatusIsCompleteNotReviewed()
        {
            _statusProcessor.Status = Status.CompleteNotReviewed;

            _processCommand
                .GetSubmissionResponsesDtoForSection(
                    _establishmentId,
                    _section,
                    true,
                    Arg.Any<CancellationToken>()
                )
                .Returns(_responsesDto);

            var result = await _router.ValidateRoute(
                _categorySlug,
                _sectionSlug,
                "error",
                _controller,
                default
            );

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChangeAnswersViewModel>(viewResult.Model);

            Assert.Equal("Change Answers", model.Title.Text);
            Assert.Equal("Test Section", model.SectionName);
            Assert.Equal(_categorySlug, model.CategorySlug);
            Assert.Equal(_sectionSlug, model.SectionSlug);
            Assert.Equal(_responsesDto, model.SubmissionResponses);
            Assert.Equal("error", model.ErrorMessage);
        }

        [Fact]
        public async Task ValidateRoute_ShouldCloneSubmission_IfCompleteReviewed()
        {
            _statusProcessor.Status = Status.CompleteReviewed;

            var inProgressSubmission = new Submission
            {
                Id = 1,
                SectionId = _section.Sys.Id,
                SectionName = _section.Name,
                EstablishmentId = _establishmentId,
                Status = SubmissionStatus.InProgress.ToString(),
                DateCreated = DateTime.UtcNow,
                Responses = new List<Response>
                {
                    new Response
                    {
                        QuestionId = 1,
                        AnswerId = 1,
                        DateCreated = DateTime.UtcNow,
                    },
                },
            };

            var latestCompletedSubmission = new Submission
            {
                Id = 2,
                SectionId = _section.Sys.Id,
                SectionName = _section.Name,
                EstablishmentId = _establishmentId,
                Completed = true,
                Responses = new List<Response>(),
                DateCreated = DateTime.UtcNow,
            };

            var clonedSubmission = new Submission
            {
                Id = 3,
                SectionId = _section.Sys.Id,
                SectionName = _section.Name,
                EstablishmentId = _establishmentId,
                Responses = new List<Response>(),
                DateCreated = DateTime.UtcNow,
            };

            var responses = inProgressSubmission.Responses;

            _dbContext.GetSubmissions.Returns(
                new List<Submission> { inProgressSubmission }.AsQueryable()
            );

            var submissions = new List<Submission> { latestCompletedSubmission };
            var firstSubmission = submissions.First();

            _dbContext
                .FirstOrDefaultAsync(
                    Arg.Any<IQueryable<Submission>>(),
                    Arg.Any<CancellationToken>()
                )
                .Returns(firstSubmission);

            _dbContext
                .ToListAsync(Arg.Any<IQueryable<Response>>(), Arg.Any<CancellationToken>())
                .Returns(responses);

            _submissionCommand
                .CloneSubmission(latestCompletedSubmission, Arg.Any<CancellationToken>())
                .Returns(clonedSubmission);

            _responsesQuery = Substitute.For<IGetLatestResponsesQuery>();

            _responsesQuery
                .GetInProgressSubmission(
                    _establishmentId,
                    _section.Sys.Id,
                    Arg.Any<CancellationToken>()
                )
                .Returns(inProgressSubmission);

            _responsesQuery
                .GetLatestCompletedSubmission(_establishmentId, _section.Sys.Id)
                .Returns(latestCompletedSubmission);

            var router = new ChangeAnswersRouter(
                _getPageQuery,
                _processCommand,
                _user,
                _statusProcessor,
                _responsesQuery,
                _submissionCommand
            );

            _statusProcessor.Section.Returns(_section);

            _dbContext.GetSubmissions.Returns(
                new List<Submission> { latestCompletedSubmission }.AsQueryable()
            );

            _processCommand
                .GetSubmissionResponsesDtoForSection(
                    _establishmentId,
                    _section,
                    true,
                    Arg.Any<CancellationToken>()
                )
                .Returns(_responsesDto);

            _statusProcessor.User.Returns(_user);
            _user.GetEstablishmentId().Returns(_establishmentId);

            var result = await router.ValidateRoute(
                _categorySlug,
                _sectionSlug,
                null,
                _controller,
                default
            );

            await _submissionCommand
                .Received(1)
                .DeleteSubmission(inProgressSubmission.Id, Arg.Any<CancellationToken>());
            await _submissionCommand
                .Received(1)
                .CloneSubmission(latestCompletedSubmission, Arg.Any<CancellationToken>());

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChangeAnswersViewModel>(viewResult.Model);
            Assert.Equal(_responsesDto, model.SubmissionResponses);
        }

        [Fact]
        public async Task ValidateRoute_ShouldRedirectToHomePage_WhenStatusIsNotStarted()
        {
            _statusProcessor.Status = Status.NotStarted;

            var result = await _router.ValidateRoute(
                _categorySlug,
                _sectionSlug,
                null,
                _controller,
                default
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(PagesController.GetPageByRouteAction, redirect.ActionName);
        }

        [Fact]
        public async Task ValidateRoute_ShouldRedirectToNextQuestion_WhenStatusUnknown()
        {
            _statusProcessor.Status = Status.InProgress;
            _statusProcessor.NextQuestion.Returns(new Question { Slug = "next-q" });

            var result = await _router.ValidateRoute(
                _categorySlug,
                _sectionSlug,
                null,
                _controller,
                default
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("next-q", redirect.RouteValues?["questionSlug"]);
        }

        [Fact]
        public async Task ProcessChangeAnswers_ShouldThrowDatabaseException_WhenResponsesNull()
        {
            _statusProcessor.Status = Status.CompleteNotReviewed;

            _processCommand
                .GetSubmissionResponsesDtoForSection(
                    _establishmentId,
                    _section,
                    true,
                    Arg.Any<CancellationToken>()
                )
                .Returns((SubmissionResponsesDto?)null);

            await Assert.ThrowsAsync<DatabaseException>(() =>
                _router.ValidateRoute(_categorySlug, _sectionSlug, null, _controller, default)
            );
        }
    }
}
