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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CheckAnswersControllerTests
    {
        private readonly CheckAnswersController _checkAnswersController;
        private readonly ProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;

        private readonly Mock<IGetLatestResponseListForSubmissionQuery> _getLatestResponseListForSubmissionQueryMock;
        private readonly Mock<IContentRepository> _contentRepositoryMock;
        private readonly Mock<GetPageQuery> _getPageQueryMock;
        private readonly Mock<ICalculateMaturityCommand> _calculateMaturityCommandMock;

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
            Mock<ILogger<CheckAnswersController>> loggerMock = new Mock<ILogger<CheckAnswersController>>();

            Mock<IQuestionnaireCacher> questionnaireCacherMock = new Mock<IQuestionnaireCacher>();
            _contentRepositoryMock = SetupRepositoryMock();

            Mock<GetSectionQuery> getSectionQueryMock = new Mock<GetSectionQuery>(_contentRepositoryMock.Object);
            _getPageQueryMock = new Mock<GetPageQuery>(questionnaireCacherMock.Object, _contentRepositoryMock.Object);

            _getLatestResponseListForSubmissionQueryMock = new Mock<IGetLatestResponseListForSubmissionQuery>();
            _calculateMaturityCommandMock = new Mock<ICalculateMaturityCommand>();

            _processCheckAnswerDtoCommand = new ProcessCheckAnswerDtoCommand(getSectionQueryMock.Object, _getLatestResponseListForSubmissionQueryMock.Object, _calculateMaturityCommandMock.Object);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _checkAnswersController = new CheckAnswersController(loggerMock.Object) { TempData = tempData };
        }

        private Mock<IContentRepository> SetupRepositoryMock()
        {
            var repositoryMock = new Mock<IContentRepository>();
            repositoryMock.Setup(repo => repo.GetEntities<Page>(It.IsAny<IGetEntitiesOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync((IGetEntitiesOptions options, CancellationToken _) =>
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
            });
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

            _getLatestResponseListForSubmissionQueryMock.Setup(m => m.GetLatestResponseListForSubmissionBy(SubmissionId)).ReturnsAsync(questionWithAnswerList);
            _contentRepositoryMock.Setup(m => m.GetEntityById<Section>(SectionId, 3, CancellationToken.None)).ReturnsAsync(_section);

            _checkAnswersController.TempData["CheckAnswersPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterCheckAnswersPage() { SubmissionId = SubmissionId, SectionId = SectionId, SectionName = SectionName });

            var result = await _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQueryMock.Object);

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

            _getLatestResponseListForSubmissionQueryMock.Setup(m => m.GetLatestResponseListForSubmissionBy(SubmissionId)).ReturnsAsync(questionWithAnswerList);
            _contentRepositoryMock.Setup(m => m.GetEntityById<Section>(SectionId, 3, CancellationToken.None)).ReturnsAsync(_section);

            _checkAnswersController.TempData["CheckAnswersPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterCheckAnswersPage() { SubmissionId = SubmissionId, SectionId = SectionId, SectionName = SectionName });

            var result = await _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQueryMock.Object);

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

            _getLatestResponseListForSubmissionQueryMock.Setup(m => m.GetLatestResponseListForSubmissionBy(SubmissionId)).ReturnsAsync(questionWithAnswerList);
            _contentRepositoryMock.Setup(m => m.GetEntityById<Section?>(SectionId, 3, CancellationToken.None)).ReturnsAsync((Section?)null);

            _checkAnswersController.TempData["CheckAnswersPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterCheckAnswersPage() { SubmissionId = SubmissionId, SectionId = SectionId, SectionName = SectionName });

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQueryMock.Object));
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_QuestionWithAnswerList_ThrowsException()
        {
            _getLatestResponseListForSubmissionQueryMock.Setup(m => m.GetLatestResponseListForSubmissionBy(SubmissionId)).ReturnsAsync((List<QuestionWithAnswer>)null!);
            _contentRepositoryMock.Setup(m => m.GetEntityById<Section?>(SectionId, 3, CancellationToken.None)).ReturnsAsync(_section);

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQueryMock.Object));
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

            _getLatestResponseListForSubmissionQueryMock.Setup(m => m.GetLatestResponseListForSubmissionBy(SubmissionId)).ReturnsAsync(questionWithAnswerList);
            _contentRepositoryMock.Setup(m => m.GetEntityById<Section?>(SectionId, 3, CancellationToken.None)).ReturnsAsync(_section);

            _checkAnswersController.TempData["CheckAnswersPage"] = Newtonsoft.Json.JsonConvert.SerializeObject(new ParameterCheckAnswersPage() { SubmissionId = SubmissionId, SectionId = null!, SectionName = SectionName });

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQueryMock.Object));
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
            Assert.NotNull(_checkAnswersController.TempData["QuestionPage"]);
            Assert.IsType<string>(_checkAnswersController.TempData["QuestionPage"]);
            var id = Newtonsoft.Json.JsonConvert.DeserializeObject<ParameterQuestionPage>(_checkAnswersController.TempData["QuestionPage"] as string ?? "")?.QuestionRef;
            Assert.Equal(question.ContentfulRef, id);
            var answerRef = Newtonsoft.Json.JsonConvert.DeserializeObject<ParameterQuestionPage>(_checkAnswersController.TempData["QuestionPage"] as string ?? "")?.AnswerRef;
            Assert.Equal(answer.ContentfulRef, answerRef);
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