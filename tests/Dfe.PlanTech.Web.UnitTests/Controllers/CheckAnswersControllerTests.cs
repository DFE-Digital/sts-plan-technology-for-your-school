using System.Diagnostics;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CheckAnswersControllerTests
    {
        private readonly CheckAnswersController _checkAnswersController;

        private readonly IProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;
        private readonly IGetSectionQuery _getSectionQuery;
        private readonly IGetPageQuery _getPageQuerySubstitute;
        private readonly IUser _user;
        private readonly ICalculateMaturityCommand _calculateMaturityCommand;

        private readonly Page _checkAnswersPage = new()
        {
            Slug = CheckAnswersController.PAGE_SLUG,
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


        public CheckAnswersControllerTests()
        {
            ILogger<CheckAnswersController> loggerSubstitute = Substitute.For<ILogger<CheckAnswersController>>();

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



            _calculateMaturityCommand = Substitute.For<ICalculateMaturityCommand>();

            _checkAnswersController = new CheckAnswersController(_user, _getSectionQuery, _processCheckAnswerDtoCommand, _getPageQuerySubstitute, _calculateMaturityCommand, loggerSubstitute);
            
            var tempData = Substitute.For<ITempDataDictionary>();
            _checkAnswersController.TempData = tempData;

        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswersPage_RedirectsToView_When_CheckAnswersDto_IsPopulated()
        {
            var result = await _checkAnswersController.CheckAnswersPage(_section.InterstitialPage.Slug);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("CheckAnswers", viewResult.ViewName);
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswersPage_ViewModel_IsPopulated_Correctly()
        {
            var result = await _checkAnswersController.CheckAnswersPage(_section.InterstitialPage.Slug);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("CheckAnswers", viewResult.ViewName);

            Assert.IsType<CheckAnswersViewModel>(viewResult.Model);

            var checkAnswersViewModel = viewResult.Model as CheckAnswersViewModel;

            Assert.NotNull(checkAnswersViewModel);
            Assert.Equal(_checkAnswersPage.Title, checkAnswersViewModel.Title);
            Assert.Equal(SUBMISSION_ID, checkAnswersViewModel.SubmissionId);

            Assert.NotNull(checkAnswersViewModel.Content);
            Assert.Equal(_checkAnswersPage.Content, checkAnswersViewModel.Content);

            Assert.IsType<CheckAnswerDto>(checkAnswersViewModel.CheckAnswerDto);

            var checkAnswerDto = checkAnswersViewModel.CheckAnswerDto;

            Assert.NotNull(checkAnswerDto);
            Assert.Equal(_questionWithAnswerList, checkAnswerDto.Responses);
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_Section_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _checkAnswersController.CheckAnswersPage("NOT THE RIGHT SLUG"));
        }


        [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_SectionId_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(null!));
        }


        [Fact]
        public async Task CheckAnswersPage_Throws_DatabaseException_When_NoResponses()
        {
            await Assert.ThrowsAsync<DatabaseException>(() => _checkAnswersController.CheckAnswersPage(_completedSection.InterstitialPage.Slug));
        }
        
        
        [Fact]
        public async Task CheckAnswersController_ConfirmCheckAnswers_RedirectsToSelfAssessmentPage_When_There_Are_No_Errors()
        {
            var result = await _checkAnswersController.ConfirmCheckAnswers(_section.InterstitialPage.Slug, SUBMISSION_ID, _section.Name);

            Assert.IsType<RedirectToActionResult>(result);

            var selfAssessmentResult = result as RedirectToActionResult;

            Assert.NotNull(selfAssessmentResult);
            Assert.True(selfAssessmentResult.ActionName == "GetByRoute");
            Assert.True(selfAssessmentResult.ControllerName == "Pages");
            Debug.Assert(selfAssessmentResult.RouteValues != null, "selfAssessmentResult.RouteValues != null");
            Assert.True(selfAssessmentResult.RouteValues["route"] is string and "/self-assessment");
        }
        
        [Fact]
        public async Task CheckAnswersController_ConfirmCheckAnswers_RedirectsToCheckAnswerPage_When_There_Is_An_Error_Calculating_Recommendations()
        {
            _calculateMaturityCommand.CalculateMaturityAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Throws(new Exception());   
            
            var result = await _checkAnswersController.ConfirmCheckAnswers(_section.InterstitialPage.Slug, SUBMISSION_ID, _section.Name);

            Assert.IsType<RedirectToActionResult>(result);

            var checkAnswerResult = result as RedirectToActionResult;

            Assert.Equal("CheckAnswers", checkAnswerResult.ControllerName);
            Assert.Equal("CheckAnswersPage", checkAnswerResult.ActionName);
            Debug.Assert(checkAnswerResult.RouteValues != null, "checkAnswerResult.RouteValues != null");
            Assert.Equal(_section.InterstitialPage.Slug, checkAnswerResult.RouteValues["sectionSlug"]);
            Assert.Equal("Unable to determine your recommendation. Please try again.", _checkAnswersController.TempData["ErrorMessage"]);
        }
        
        [Fact]
        public async Task CheckAnswersController_CheckAnswersPage_Generates_View_With_Error_When_There_Is_An_Error()
        {
            _checkAnswersController.TempData["ErrorMessage"] = "Unable to determine your recommendation. Please try again.";
            
            var result = await _checkAnswersController.CheckAnswersPage(_section.InterstitialPage.Slug);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("CheckAnswers", viewResult.ViewName);

            Assert.IsType<CheckAnswersViewModel>(viewResult.Model);

            var checkAnswersViewModel = viewResult.Model as CheckAnswersViewModel;

            Assert.NotNull(checkAnswersViewModel);
            Assert.Equal(_checkAnswersPage.Title, checkAnswersViewModel.Title);
            Assert.Equal(SUBMISSION_ID, checkAnswersViewModel.SubmissionId);

            Assert.NotNull(checkAnswersViewModel.Content);
            Assert.Equal(_checkAnswersPage.Content, checkAnswersViewModel.Content);

            Assert.IsType<CheckAnswerDto>(checkAnswersViewModel.CheckAnswerDto);

            var checkAnswerDto = checkAnswersViewModel.CheckAnswerDto;

            Assert.NotNull(checkAnswerDto);
            Assert.Equal(_questionWithAnswerList, checkAnswerDto.Responses);
            
            Assert.NotNull(checkAnswersViewModel.ErrorMessage);
            Assert.Equal("Unable to determine your recommendation. Please try again.", checkAnswersViewModel.ErrorMessage);
        }
        
    }
}
