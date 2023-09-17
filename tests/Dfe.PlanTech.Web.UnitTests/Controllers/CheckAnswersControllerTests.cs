using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Responses.Commands;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
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

        private const int ESTABLISHMENT_ID = 3;
        private const int USER_ID = 7;
        private const int SUBMISSION_ID = 1;

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
            _getSectionQuery.GetSectionBySlug(_section.InterstitialPage.Slug).Returns((callInfo) =>
            {
                var sectionArg = callInfo.ArgAt<string>(0);

                return sectionArg == _section.InterstitialPage.Slug ? _section : null;
            });

            _processCheckAnswerDtoCommand = Substitute.For<IProcessCheckAnswerDtoCommand>();
            _processCheckAnswerDtoCommand.GetCheckAnswerDtoForSectionId(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                                        .Returns((callInfo) =>
                                        {
                                            string sectionId = callInfo.ArgAt<string>(1);

                                            if (sectionId != _section.Sys.Id) return null;

                                            return new CheckAnswerDto()
                                            {
                                                QuestionAnswerList = _questionWithAnswerList,
                                                SubmissionId = SUBMISSION_ID
                                            };
                                        });

            _checkAnswersController = new CheckAnswersController(loggerSubstitute);
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswersPage_RedirectsToView_When_CheckAnswersDto_IsPopulated()
        {
            var result = await _checkAnswersController.CheckAnswersPage(_section.InterstitialPage.Slug, _user, _getSectionQuery, _processCheckAnswerDtoCommand, _getPageQuerySubstitute);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("CheckAnswers", viewResult.ViewName);
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswersPage_ViewModel_IsPopulated_Correctly()
        {
            var result = await _checkAnswersController.CheckAnswersPage(_section.InterstitialPage.Slug, _user, _getSectionQuery, _processCheckAnswerDtoCommand, _getPageQuerySubstitute);

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
            Assert.Equal(_questionWithAnswerList, checkAnswerDto.QuestionAnswerList);
        }

/*         [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_Section_ThrowsException()
        {
            List<QuestionWithAnswer> questionWithAnswerList = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = "QuestionRef-1",
                    QuestionText = "Question Text",
                    AnswerRef = "AnswerRef-1",
                    AnswerText = "Answer Text"
                }
            };

            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SUBMISSION_ID).Returns(questionWithAnswerList);
            _contentRepositorySubstitute.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns((Section?)null);

            _checkAnswersController.TempData[TempDataConstants.CheckAnswers] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataCheckAnswers() { SubmissionId = SUBMISSION_ID, SectionId = SectionId, SectionName = SectionName });

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQuerySubstitute));
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_QuestionWithAnswerList_ThrowsException()
        {
            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SUBMISSION_ID).ReturnsNull();
            _contentRepositorySubstitute.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns(_section);

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQuerySubstitute));
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_SectionId_ThrowsException()
        {
            List<QuestionWithAnswer> questionWithAnswerList = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = "QuestionRef-1",
                    QuestionText = "Question Text",
                    AnswerRef = "AnswerRef-1",
                    AnswerText = "Answer Text"
                }
            };

            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SUBMISSION_ID).Returns(Task.FromResult(questionWithAnswerList));
            _contentRepositorySubstitute.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns(_section);

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQuerySubstitute));
        }

        [Fact]
        public async Task CheckAnswersController_ChangeAnswer_RedirectsToView()
        {
            Domain.Questions.Models.Question question = new Domain.Questions.Models.Question() { ContentfulRef = "QuestionRef-1", QuestionText = "Question Text" };
            Domain.Answers.Models.Answer answer = new Domain.Answers.Models.Answer() { ContentfulRef = "AnswerRef-1", AnswerText = "Answer Text" };


            var result = await _checkAnswersController.ChangeAnswer(question.ContentfulRef, answer.ContentfulRef, SUBMISSION_ID, string.Empty);

            Assert.IsType<RedirectToRouteResult>(result);

            var redirectToActionResult = result as RedirectToRouteResult;

            Assert.NotNull(redirectToActionResult);
            Assert.NotNull(_checkAnswersController.TempData[TempDataConstants.Questions]);
            Assert.IsType<string>(_checkAnswersController.TempData[TempDataConstants.Questions]);
            var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataQuestions>(_checkAnswersController.TempData[TempDataConstants.Questions] as string ?? "")?.QuestionRef;
            Assert.Equal(question.ContentfulRef, id);
            var answerRef = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataQuestions>(_checkAnswersController.TempData[TempDataConstants.Questions] as string ?? "")?.AnswerRef;
            Assert.Equal(answer.ContentfulRef, answerRef);
        }

        [Fact]
        public async Task ConfirmCheckAnswers_RedirectsToSelfAssessment_WhenMaturityIsLargerThan1()
        {
            var result = await _checkAnswersController.ConfirmCheckAnswers(SUBMISSION_ID, SectionName, _processCheckAnswerDtoCommand);

            Assert.IsType<RedirectToActionResult>(result);

            var res = result as RedirectToActionResult;

            if (res != null)
            {
                Assert.True(res.ActionName == "GetByRoute");
                Assert.True(res.ControllerName == "Pages");
            }
        }
 */    }
}