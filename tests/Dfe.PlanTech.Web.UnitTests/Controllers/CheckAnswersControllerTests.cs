using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CheckAnswersControllerTests
    {
        private readonly CheckAnswersController _checkAnswersController;
        private readonly ProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;

        private IGetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQueryMock;
        private IContentRepository _contentRepositoryMock;
        private GetPageQuery _getPageQueryMock;
        private ICalculateMaturityCommand _calculateMaturityCommandMock;

        private const int SubmissionId = 1;
        private const string SectionId = "SectionId";
        private const string SectionName = "SectionName";

        private readonly Page[] _pages = new Page[]
        {
            new Page()
            {
                Slug = "check-answers",
                Title = new Title() { Text = "Title Text" },
                Content = new ContentComponent[] { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Header Text" }}
            },
        };
        private readonly Section _section = new Section()
        {
            Name = SectionName,
            Questions = new Question[]
            {
                new Question()
                {
                    Sys = new SystemDetails() { Id = "QuestionRef-1"},
                    Answers = new Answer[]
                    {
                        new Answer()
                        {
                            Sys = new SystemDetails() { Id = "AnswerRef-1" },
                            NextQuestion = null
                        }
                    }
                }
            },
        };

        public CheckAnswersControllerTests()
        {
            ILogger<CheckAnswersController> loggerMock = Substitute.For<ILogger<CheckAnswersController>>();

            IQuestionnaireCacher questionnaireCacherMock = Substitute.For<IQuestionnaireCacher>();
            _contentRepositoryMock = SetupRepositoryMock();

            GetSectionQuery getSectionQueryMock = Substitute.For<GetSectionQuery>(_contentRepositoryMock);
            _getPageQueryMock = Substitute.For<GetPageQuery>(questionnaireCacherMock, _contentRepositoryMock);

            _getLatestResponseListForSubmissionQueryMock = Substitute.For<IGetLatestResponseListForSubmissionQuery>();
            _calculateMaturityCommandMock = Substitute.For<ICalculateMaturityCommand>();

            ITempDataDictionary tempDataMock = Substitute.For<ITempDataDictionary>();

            _processCheckAnswerDtoCommand = new ProcessCheckAnswerDtoCommand(getSectionQueryMock, _getLatestResponseListForSubmissionQueryMock, _calculateMaturityCommandMock);

            _checkAnswersController = new CheckAnswersController(loggerMock);

            _checkAnswersController.TempData = tempDataMock;
        }

        private IContentRepository SetupRepositoryMock()
        {
            var repositoryMock = Substitute.For<IContentRepository>();
            repositoryMock.GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((IGetEntitiesOptions options, CancellationToken _) =>
            {
                if (options?.Queries != null)
                {
                    foreach (var query in options.Queries)
                    {
                        if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                        {
                            return _pages.Where(page => page.Slug == equalsQuery.Value);
                        }
                    }
                }
                return Array.Empty<Page>();
            }));
            return repositoryMock;
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswersPage_RedirectsToView_When_CheckAnswersDto_IsPopulated()
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

            _getLatestResponseListForSubmissionQueryMock.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(Task.FromResult(questionWithAnswerList));
            _contentRepositoryMock.GetEntityById<Section>(SectionId, 3, CancellationToken.None).Returns(Task.FromResult(_section));

            var result = await _checkAnswersController.CheckAnswersPage(SubmissionId, SectionId, SectionName, _processCheckAnswerDtoCommand, _getPageQueryMock);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("CheckAnswers", viewResult.ViewName);
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswersPage_ViewModel_IsPopulated_Correctly()
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

            _getLatestResponseListForSubmissionQueryMock.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(Task.FromResult(questionWithAnswerList));
            _contentRepositoryMock.GetEntityById<Section>(SectionId, 3, CancellationToken.None).Returns(Task.FromResult(_section));

            var result = await _checkAnswersController.CheckAnswersPage(SubmissionId, SectionId, SectionName, _processCheckAnswerDtoCommand, _getPageQueryMock);

            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            Assert.NotNull(viewResult);
            Assert.Equal("CheckAnswers", viewResult.ViewName);

            Assert.IsType<CheckAnswersViewModel>(viewResult.Model);

            var checkAnswersViewModel = viewResult.Model as CheckAnswersViewModel;

            Assert.NotNull(checkAnswersViewModel);
            Assert.Equal("Title Text", checkAnswersViewModel.Title.Text);
            Assert.Equal(SubmissionId, checkAnswersViewModel.SubmissionId);

            Assert.IsType<Header>(checkAnswersViewModel.Content[0]);

            var content = checkAnswersViewModel.Content[0] as Header;

            Assert.NotNull(content);
            Assert.Equal("Header Text", content.Text);

            Assert.IsType<CheckAnswerDto>(checkAnswersViewModel.CheckAnswerDto);

            var checkAnswerDto = checkAnswersViewModel.CheckAnswerDto;

            Assert.NotNull(checkAnswerDto);
            Assert.Equal("QuestionRef-1", checkAnswerDto.QuestionAnswerList[0].QuestionRef);
            Assert.Equal("Question Text", checkAnswerDto.QuestionAnswerList[0].QuestionText);
            Assert.Equal("AnswerRef-1", checkAnswerDto.QuestionAnswerList[0].AnswerRef);
            Assert.Equal("Answer Text", checkAnswerDto.QuestionAnswerList[0].AnswerText);
        }

        [Fact]
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

            _getLatestResponseListForSubmissionQueryMock.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(Task.FromResult(questionWithAnswerList));
            _contentRepositoryMock.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns(Task.FromResult((Section?)null));

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId, SectionId, SectionName, _processCheckAnswerDtoCommand, _getPageQueryMock));
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_QuestionWithAnswerList_ThrowsException()
        {
            _getLatestResponseListForSubmissionQueryMock.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(Task.FromResult((List<QuestionWithAnswer>)null!));
            _contentRepositoryMock.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns(Task.FromResult(_section));

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId, SectionId, SectionName, _processCheckAnswerDtoCommand, _getPageQueryMock));
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

            _getLatestResponseListForSubmissionQueryMock.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(Task.FromResult(questionWithAnswerList));
            _contentRepositoryMock.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns(Task.FromResult(_section));

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId, null!, SectionName, _processCheckAnswerDtoCommand, _getPageQueryMock));
        }

        [Fact]
        public void CheckAnswersController_ChangeAnswer_RedirectsToView()
        {
            Domain.Questions.Models.Question question = new Domain.Questions.Models.Question() { ContentfulRef = "QuestionRef-1", QuestionText = "Question Text" };
            Domain.Answers.Models.Answer answer = new Domain.Answers.Models.Answer() { ContentfulRef = "AnswerRef-1", AnswerText = "Answer Text" };

            var result = _checkAnswersController.ChangeAnswer(question.ContentfulRef, answer.ContentfulRef, SubmissionId);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
            Assert.Equal(question.ContentfulRef, id.Value);
            var answerRef = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "answerRef");
            Assert.Equal(answer.ContentfulRef, answerRef.Value);
        }

        [Fact]
        public async Task ConfirmCheckAnswers_RedirectsToSelfAssessment_WhenMaturityIsLargerThan1()
        {
            var result = await _checkAnswersController.ConfirmCheckAnswers(SubmissionId, SectionName, _processCheckAnswerDtoCommand);

            Assert.IsType<RedirectToActionResult>(result);

            var res = result as RedirectToActionResult;

            if (res != null)
            {
                Assert.True(res.ActionName == "GetByRoute");
                Assert.True(res.ControllerName == "Pages");
            }
        }
    }
}