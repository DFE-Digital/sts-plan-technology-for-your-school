using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CheckAnswersControllerTests
    {
        private readonly CheckAnswersController _checkAnswersController;

        /*
                private readonly IProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;
                private readonly IGetSectionQuery _getSectionQuery;
                private readonly IGetPageQuery _getPageQuerySubstitute;
                private readonly IUser _user;

                private readonly Page _checkAnswersPage = new()
                {
                    Slug = CheckAnswersController.CheckAnswersPageSlug,
                    Title = new Title() { Text = "Title Text" },
                    Content = new ContentComponent[] { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Header Text" } }
                };

                private static readonly Page InterstitialPage = new()
                {
                    Slug = "section-slug",
                };

                private readonly Section _section = new()
                {
                    Name = "Section Name",
                    InterstitialPage = InterstitialPage,
                    Sys = new()
                    {
                        Id = "SectionId"
                    },
                    Questions = new Question[]
                    {
                        new()
                        {
                            Sys = new SystemDetails() { Id = "QuestionRef-1"},
                            Answers = new Answer[]
                            {
                                new()
                                {
                                    Sys = new SystemDetails() { Id = "AnswerRef-1" },
                                    NextQuestion = null
                                }
                            }
                        }
                    },
                };
                private readonly Section _completedSection = new()
                {
                    InterstitialPage = new Page()
                    {
                        Slug = COMPLETED_SECTION_SLUG
                    },
                    Sys = new()
                    {
                        Id = "CompletedSectionId"
                    }
                };

                private const int ESTABLISHMENT_ID = 3;
                private const int USER_ID = 7;
                private const int SUBMISSION_ID = 1;
                private const string COMPLETED_SECTION_SLUG = "COMPLETED";

                private readonly List<QuestionWithAnswer> _questionWithAnswerList = new()
                    {
                        new QuestionWithAnswer()
                        {
                            QuestionRef = "QuestionRef-1",
                            QuestionText = "Question Text",
                            AnswerRef = "AnswerRef-1",
                            AnswerText = "Answer Text"
                        }
                    };
                    */

        private readonly ICalculateMaturityCommand _calculateMaturityCommand;
        private readonly ICheckAnswersRouter _checkAnswersRouter;

        public CheckAnswersControllerTests()
        {
            var loggerSubstitute = Substitute.For<ILogger<CheckAnswersController>>();
            _calculateMaturityCommand = Substitute.For<ICalculateMaturityCommand>();
            _checkAnswersRouter = Substitute.For<ICheckAnswersRouter>();

            _checkAnswersController = new CheckAnswersController(loggerSubstitute)
            {
                ControllerContext = ControllerHelpers.SubstituteControllerContext()
            };
            
            /*
                        _getPageQuerySubstitute = Substitute.For<IGetPageQuery>();
                        _getPageQuerySubstitute.GetPageBySlug("check-answers", Arg.Any<CancellationToken>()).Returns(_checkAnswersPage);

                        _user = Substitute.For<IUser>();
                        _user.GetEstablishmentId().Returns(ESTABLISHMENT_ID);
                        _user.GetCurrentUserId().Returns(Task.FromResult(USER_ID as int?));

                        _getSectionQuery = Substitute.For<IGetSectionQuery>();
                        _getSectionQuery.GetSectionBySlug(Arg.Any<string>()).Returns((callInfo) =>
                        {
                            var sectionArg = callInfo.ArgAt<string>(0);

                            if (sectionArg == _section.InterstitialPage.Slug)
                            {
                                return _section;
                            }
                            else if (sectionArg == _completedSection.InterstitialPage.Slug)
                            {
                                return _completedSection;
                            }

                            return null;
                        });

                        _processCheckAnswerDtoCommand = Substitute.For<IProcessCheckAnswerDtoCommand>();
                        _processCheckAnswerDtoCommand.GetCheckAnswerDtoForSection(Arg.Any<int>(), Arg.Any<Section>(), Arg.Any<CancellationToken>())
                                                    .Returns((callInfo) =>
                                                    {
                                                        Section section = callInfo.ArgAt<Section>(1);

                                                        if (section != _section) return null;

                                                        return new CheckAnswerDto()
                                                        {
                                                            Responses = _questionWithAnswerList,
                                                            SubmissionId = SUBMISSION_ID
                                                        };
                                                    });
            */
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task CheckAnswersPage_Should_ThrowException_When_SectionSlug_NullOrEmpty(string? section)
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(section!, _checkAnswersRouter, default));
        }

        //Confirm check answers tests:
        //Calculates maturity whenv alid args

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task ConfirmAnswers_Should_ThrowException_When_SubmissionId_OutOfRange(int submissionId)
        {
            await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(() => _checkAnswersController.ConfirmCheckAnswers(submissionId, "section name", _calculateMaturityCommand));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ConfirmAnswers_Should_ThrowException_When_SectionName_NullOrEmpty(string? sectionName)
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.ConfirmCheckAnswers(1, sectionName!, _calculateMaturityCommand));
        }

        [Fact]
        public async Task ConfirmAnswers_Should_CalculateMaturity_When_ArgsValid()
        {
            int submissionId = 1;
            int? submissionIdResult = null;

            _calculateMaturityCommand.CalculateMaturityAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                                        .Returns((callinfo) =>
                                        {
                                            submissionIdResult = callinfo.ArgAt<int>(0);
                                            return 2;
                                        });

            var result = await _checkAnswersController.ConfirmCheckAnswers(submissionId, "section name", _calculateMaturityCommand);

            Assert.Equal(submissionId, submissionIdResult);
        }

        [Fact]
        public async Task ConfirmAnswers_Should_Redirect_To_SelfAssessmentPage()
        {
            var result = await _checkAnswersController.ConfirmCheckAnswers(1, "section name", _calculateMaturityCommand);

            var redirectToActionResult = result as RedirectToActionResult;
            if (redirectToActionResult == null)
            {
                Assert.Fail("Not redirect to action result");
            }

            Assert.Equal(PagesController.ControllerName, redirectToActionResult.ControllerName);
            Assert.Equal(PagesController.GetPageByRouteAction, redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            Assert.True(redirectToActionResult.RouteValues.ContainsKey("route"));
            Assert.True(redirectToActionResult.RouteValues["route"] is string s && s == "/self-assessment");
        }
    }
}
