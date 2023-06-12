using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetQuestionQuery : ContentRetriever
{
    public GetQuestionQuery(IContentRepository repository) : base(repository)
    {
    }

    public async Task<Question?> GetQuestionById(string id)
    {
        var question = await repository.GetEntityById<Question>(id, 3);

        return question;
    }
}
