using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class CheckAnswersControllerTests
    {
        private readonly CheckAnswersController _checkAnswersController;
        private readonly ProcessCheckAnswerDtoCommand _processCheckAnswerDtoCommand;

        private IGetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuerySubstitute;
        private IContentRepository _contentRepositorySubstitute;
        private GetPageQuery _getPageQuerySubstitute;
        private ICalculateMaturityCommand _calculateMaturityCommandSubstitute;

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
            ILogger<CheckAnswersController> loggerSubstitute = Substitute.For<ILogger<CheckAnswersController>>();

            IQuestionnaireCacher questionnaireCacherSubstitute = Substitute.For<IQuestionnaireCacher>();
            _contentRepositorySubstitute = SetupRepositorySubstitute();

            GetSectionQuery getSectionQuerySubstitute = Substitute.For<GetSectionQuery>(_contentRepositorySubstitute);
            _getPageQuerySubstitute = Substitute.For<GetPageQuery>(questionnaireCacherSubstitute, _contentRepositorySubstitute);

            _getLatestResponseListForSubmissionQuerySubstitute = Substitute.For<IGetLatestResponseListForSubmissionQuery>();
            _calculateMaturityCommandSubstitute = Substitute.For<ICalculateMaturityCommand>();

            ITempDataDictionary tempDataSubstitute = Substitute.For<ITempDataDictionary>();

            _processCheckAnswerDtoCommand = new ProcessCheckAnswerDtoCommand(getSectionQuerySubstitute, _getLatestResponseListForSubmissionQuerySubstitute, _calculateMaturityCommandSubstitute);

            _checkAnswersController = new CheckAnswersController(loggerSubstitute, null);

            _checkAnswersController.TempData = tempDataSubstitute;
        }

        private IContentRepository SetupRepositorySubstitute()
        {
            var repositorySubstitute = Substitute.For<IContentRepository>();
            repositorySubstitute.GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns((callInfo) =>
            {
                IGetEntitiesOptions options = (IGetEntitiesOptions)callInfo[0];
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
            return repositorySubstitute;
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

            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(questionWithAnswerList);
            _contentRepositorySubstitute.GetEntityById<Section>(SectionId, 3, CancellationToken.None).Returns(_section);

            _checkAnswersController.TempData[TempDataConstants.CheckAnswers] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataCheckAnswers() { SubmissionId = SubmissionId, SectionId = SectionId, SectionName = SectionName });

            var result = await _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQuerySubstitute);

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

            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(questionWithAnswerList);
            _contentRepositorySubstitute.GetEntityById<Section>(SectionId, 3, CancellationToken.None).Returns(_section);

            _checkAnswersController.TempData[TempDataConstants.CheckAnswers] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataCheckAnswers() { SubmissionId = SubmissionId, SectionId = SectionId, SectionName = SectionName });

            var result = await _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQuerySubstitute);

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

            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(questionWithAnswerList);
            _contentRepositorySubstitute.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns((Section?)null);

            _checkAnswersController.TempData[TempDataConstants.CheckAnswers] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataCheckAnswers() { SubmissionId = SubmissionId, SectionId = SectionId, SectionName = SectionName });

            await Assert.ThrowsAnyAsync<NullReferenceException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQuerySubstitute));
        }

        [Fact]
        public async Task CheckAnswersController_CheckAnswers_Null_QuestionWithAnswerList_ThrowsException()
        {
            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SubmissionId).ReturnsNull();
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

            _getLatestResponseListForSubmissionQuerySubstitute.GetLatestResponseListForSubmissionBy(SubmissionId).Returns(Task.FromResult(questionWithAnswerList));
            _contentRepositorySubstitute.GetEntityById<Section?>(SectionId, 3, CancellationToken.None).Returns(_section);

            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(_processCheckAnswerDtoCommand, _getPageQuerySubstitute));
        }

        [Fact]
        public void CheckAnswersController_ChangeAnswer_RedirectsToView()
        {
            Domain.Questions.Models.Question question = new Domain.Questions.Models.Question() { ContentfulRef = "QuestionRef-1", QuestionText = "Question Text" };
            Domain.Answers.Models.Answer answer = new Domain.Answers.Models.Answer() { ContentfulRef = "AnswerRef-1", AnswerText = "Answer Text" };

            var result = _checkAnswersController.ChangeAnswer(question.ContentfulRef, answer.ContentfulRef, SubmissionId, string.Empty);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
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