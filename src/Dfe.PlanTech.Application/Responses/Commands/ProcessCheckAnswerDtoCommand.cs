using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interfaces;

namespace Dfe.PlanTech.Application.Responses.Commands;

public class ProcessCheckAnswerDtoCommand : IProcessCheckAnswerDtoCommand
{
    private readonly IGetSectionQuery _getSectionQuery;
    private readonly IGetLatestResponsesQuery _getLatestResponseListForSubmissionQuery;

    public ProcessCheckAnswerDtoCommand(IGetSectionQuery getSectionQuery, IGetLatestResponsesQuery getLatestResponseListForSubmissionQuery)
    {
        _getSectionQuery = getSectionQuery;
        _getLatestResponseListForSubmissionQuery = getLatestResponseListForSubmissionQuery;
    }

    public async Task<CheckAnswerDto?> GetCheckAnswerDtoForSection(int establishmentId, Section section, CancellationToken cancellationToken = default)
    {
        var questionWithAnswerList = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(establishmentId, section.Sys.Id, cancellationToken);
        if (questionWithAnswerList.Value.Responses == null || !questionWithAnswerList.Value.Responses.Any())
        {
            return null;
        }

        return RemoveDetachedQuestions(questionWithAnswerList.Value.Responses, section, questionWithAnswerList.Value.Id);
    }

    private static CheckAnswerDto RemoveDetachedQuestions(List<QuestionWithAnswer> questionWithAnswerList, Section section, int submissionId)
    {
        if (questionWithAnswerList == null) throw new ArgumentNullException(nameof(questionWithAnswerList));
        if (section == null) throw new ArgumentNullException(nameof(section));

        CheckAnswerDto checkAnswerDto = new()
        {
            SubmissionId = submissionId,
            QuestionAnswerList = new List<QuestionWithAnswer>(questionWithAnswerList.Count)
        };

        var questionWithAnswerMap = questionWithAnswerList.ToDictionary(questionWithAnswer => questionWithAnswer.QuestionRef,
                                                                        questionWithAnswer => questionWithAnswer);

        Question? node = section.Questions[0];

        while (node != null)
        {
            if (questionWithAnswerMap.TryGetValue(node.Sys.Id, out QuestionWithAnswer? questionWithAnswer))
            {
                var answer = Array.Find(node.Answers, answer => answer.Sys.Id == questionWithAnswer.AnswerRef);
                questionWithAnswer = questionWithAnswer with
                {
                    AnswerText = answer?.Text ?? questionWithAnswer.AnswerText,
                    QuestionText = node.Text,
                    QuestionSlug = node.Slug
                };

                checkAnswerDto.QuestionAnswerList.Add(questionWithAnswer);
                node = node.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(questionWithAnswer.AnswerRef))?.NextQuestion;
            }
            else node = null;
        }

        return checkAnswerDto;
    }
}
