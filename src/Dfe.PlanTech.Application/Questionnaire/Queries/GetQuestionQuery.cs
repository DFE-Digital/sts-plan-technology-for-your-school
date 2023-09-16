using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

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
        var latestQuestionWithAnswer = await _getResponseQuery.GetLatestResponse(establishmentId, section.Sys.Id);

        //When there's no response for section yet == section not started == return first question for section
        if (latestQuestionWithAnswer == null)
        {
            return section.Questions.FirstOrDefault();
        }

        var question = Array.Find(section.Questions, question => question.Sys.Id == latestQuestionWithAnswer.QuestionRef) ?? 
                        throw new Exception("Not find question");
                        
        var nextQuestion = Array.Find(question.Answers, answer => answer.Sys.Id.Equals(latestQuestionWithAnswer.AnswerRef))?.NextQuestion;

        return nextQuestion;
    }

    private void UpdateSectionTitle(string section)
    {
        var cached = _cacher.Cached ?? new QuestionnaireCache();
        cached = cached with { CurrentSectionTitle = section };

        _cacher.SaveCache(cached);
    }
}
