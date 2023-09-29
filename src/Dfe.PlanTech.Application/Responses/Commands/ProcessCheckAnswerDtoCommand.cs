using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Interface;
using Dfe.PlanTech.Domain.Responses.Interfaces;

namespace Dfe.PlanTech.Application.Responses.Commands;

public class ProcessCheckAnswerDtoCommand : IProcessCheckAnswerDtoCommand
{
    private readonly IGetLatestResponsesQuery _getLatestResponseListForSubmissionQuery;

    public ProcessCheckAnswerDtoCommand(IGetLatestResponsesQuery getLatestResponseListForSubmissionQuery)
    {
        _getLatestResponseListForSubmissionQuery = getLatestResponseListForSubmissionQuery;
    }

    public async Task<CheckAnswerDto?> GetCheckAnswerDtoForSection(int establishmentId, Section section, CancellationToken cancellationToken = default)
    {
        var checkAnswerDto = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(establishmentId, section.Sys.Id, cancellationToken);
        if (checkAnswerDto?.Responses == null || !checkAnswerDto.Responses.Any())
        {
            return null;
        }

        return RemoveDetachedQuestions(checkAnswerDto, section);
    }

    private static CheckAnswerDto RemoveDetachedQuestions(CheckAnswerDto checkAnswerDto, Section section)
    {
        if (checkAnswerDto == null) throw new ArgumentNullException(nameof(checkAnswerDto));
        if (section == null) throw new ArgumentNullException(nameof(section));


        var questionWithAnswerMap = checkAnswerDto.Responses.ToDictionary(questionWithAnswer => questionWithAnswer.QuestionRef,
                                                                        questionWithAnswer => questionWithAnswer);

        var attachedQuestions = new List<QuestionWithAnswer>(checkAnswerDto.Responses.Count);

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

                attachedQuestions.Add(questionWithAnswer);
                node = Array.Find(node.Answers, answer => answer.Sys.Id.Equals(questionWithAnswer.AnswerRef))?.NextQuestion;
            }
            else node = null;
        }

        return new CheckAnswerDto()
        {
            SubmissionId = checkAnswerDto.SubmissionId,
            Responses = attachedQuestions
        };
    }
}
