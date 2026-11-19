using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.UnitTests.Shared.Builders;

public class EntryBuilders
{
    public static QuestionnaireAnswerEntry BuildAnswer(int answerNumber) =>
        new() { Sys = new($"Q{answerNumber}") };

    public static QuestionnaireAnswerEntry BuildAnswer(string answerRef) =>
        new() { Sys = new(answerRef) };

    public static QuestionnaireQuestionEntry BuildQuestion(int questionNumber) =>
        new() { Sys = new($"Q{questionNumber}") };

    public static QuestionnaireQuestionEntry BuildQuestion(string questionRef) =>
        new() { Sys = new(questionRef) };

    public static List<QuestionnaireQuestionEntry> BuildQuestions(
        IEnumerable<int> questionNumbers
    ) => questionNumbers.Select(BuildQuestion).ToList();

    public static RecommendationChunkEntry BuildRecommendationChunk(
        string @ref,
        string questionRef,
        IEnumerable<string>? completingAnswerRefs = null,
        IEnumerable<string>? inProgressAnswerRefs = null
    ) =>
        new()
        {
            Sys = new(@ref),
            Question = new() { Sys = new(questionRef) },
            CompletingAnswers =
                completingAnswerRefs
                    ?.Select(answerRef => new QuestionnaireAnswerEntry() { Sys = new(answerRef) })
                    .ToList()
                ?? [],
            InProgressAnswers =
                inProgressAnswerRefs
                    ?.Select(answerRef => new QuestionnaireAnswerEntry() { Sys = new(answerRef) })
                    .ToList()
                ?? [],
        };

    public static QuestionnaireSectionEntry BuildSection(
        RecommendationChunkEntry recommendationChunk,
        List<QuestionnaireQuestionEntry> questions
    ) => new() { CoreRecommendations = [recommendationChunk], Questions = questions };

    public static QuestionnaireSectionEntry BuildSection(
        Dictionary<int, IEnumerable<int>> questionAnswerIds,
        Dictionary<int, int> answerNextQuestionIds
    )
    {
        Dictionary<int, QuestionnaireQuestionEntry> questions = [];
        foreach (var questionId in questionAnswerIds.Keys)
        {
            var question = new QuestionnaireQuestionEntry() { Sys = new($"Q{questionId}") };

            questions.Add(questionId, question);
        }

        foreach (var questionId in questionAnswerIds.Keys)
        {
            List<QuestionnaireAnswerEntry> answers = [];
            foreach (var answerId in questionAnswerIds[questionId])
            {
                var nextQuestionId = answerNextQuestionIds[answerId];

                answers.Add(
                    new()
                    {
                        Sys = new($"A{answerId}"),
                        NextQuestion = questions.TryGetValue(nextQuestionId, out var question)
                            ? question
                            : null,
                    }
                );
            }

            questions[questionId].Answers = answers;
        }

        return new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("SEC"),
            Questions = questions.Values,
        };
    }
}
