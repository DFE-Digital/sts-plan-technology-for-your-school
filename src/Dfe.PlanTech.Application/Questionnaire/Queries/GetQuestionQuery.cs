using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetQuestionQuery : ContentRetriever
{
    private readonly IQuestionnaireCacher _cacher;
    private readonly IGetLatestResponseListForSubmissionQuery _getResponseQuery;
    private readonly GetSectionQuery _getSectionQuery;

    public GetQuestionQuery(IQuestionnaireCacher cacher,
                            IGetLatestResponseListForSubmissionQuery getResponseQuery,
                            GetSectionQuery getSectionQuery,
                            IContentRepository repository) : base(repository)
    {
        _cacher = cacher;
        _getResponseQuery = getResponseQuery;
        _getSectionQuery = getSectionQuery;
    }

    public async Task<Question?> GetQuestionById(string id, CancellationToken cancellationToken = default)
    {
        var question = await repository.GetEntityById<Question>(id, 3, cancellationToken);

        return question;
    }

    public Task<Question?> GetQuestionById(string id, string? section, CancellationToken cancellationToken = default)
    {
        if (section != null)
        {
            UpdateSectionTitle(section);
        }

        return GetQuestionById(id, cancellationToken);
    }

    public async Task<QuestionWithSectionDto?> GetQuestionBySlug(string sectionSlug, string questionSlug, CancellationToken cancellationToken = default)
    {
        var section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken);

        if (section == null) return null;

        var question = Array.Find(section.Questions, question => question.Slug == questionSlug) ??
                        throw new KeyNotFoundException($"Unable to find question with slug {questionSlug} under section {sectionSlug}");

        return new QuestionWithSectionDto(question,
                                            SectionId: section.Sys.Id,
                                            SectionName: section.Name,
                                            SectionSlug: section.InterstitialPage.Slug);
    }

    public async Task<QuestionWithSectionDto?> GetQuestionByIdNew(string sectionId, string questionId, CancellationToken cancellationToken = default)
    {
        var section = await _getSectionQuery.GetSectionById(sectionId, cancellationToken);

        if (section == null) return null;

        var question = Array.Find(section.Questions, question => question.Sys.Id == questionId) ??
                        throw new KeyNotFoundException($"Unable to find question with slug {questionId} under section {questionId}");

        return new QuestionWithSectionDto(question,
                                            SectionId: section.Sys.Id,
                                            SectionName: section.Name,
                                            SectionSlug: section.InterstitialPage.Slug);
    }

    //TODO: move to another class 
    public async Task<Question?> GetNextUnansweredQuestion(int establishmentId, Section section, CancellationToken cancellationToken = default)
    {
        var answeredQuestions = await _getResponseQuery.GetLatestResponses(establishmentId, section.Sys.Id, cancellationToken);

        //When there's no response for section yet == section not started == return first question for section
        if (answeredQuestions.Responses == null || answeredQuestions.Responses.Count == 0) return section.Questions.FirstOrDefault();

        return GetNextQuestion(section, answeredQuestions.Responses);
    }

    //TODO: Refactor this with the ProcessCheckAnswerCommand logic
    private static Question? GetNextQuestion(Section section, List<QuestionWithAnswer> responses)
    {
        Question? node = section.Questions[0];

        while (node != null)
        {
            var response = responses.FirstOrDefault(response => response.QuestionRef == node.Sys.Id);
            if (response != null)
            {
                var answer = Array.Find(node.Answers, answer => answer.Sys.Id == response.AnswerRef);
                
                response = response with
                {
                    AnswerText = answer?.Text ?? response.AnswerText,
                    QuestionText = node.Text,
                    QuestionSlug = node.Slug
                };

                node = node.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(response.AnswerRef))?.NextQuestion;
            }
            else
            {
                return node;
            }
        }

        return null;
    }

    private void UpdateSectionTitle(string section)
    {
        var cached = _cacher.Cached ?? new QuestionnaireCache();
        cached = cached with { CurrentSectionTitle = section };

        _cacher.SaveCache(cached);
    }
}
