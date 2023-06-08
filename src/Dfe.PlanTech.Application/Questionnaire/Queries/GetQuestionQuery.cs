using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetQuestionQuery : ContentRetriever
{
    public GetQuestionQuery(IContentRepository repository) : base(repository)
    {
    }

    public async Task<Question> GetQuestionById(string id)
    {
        var options = new GetEntitiesOptions(3, new[] { new ContentQueryEquals() { Field = "sys.id", Value = id } });
        var questions = await repository.GetEntities<Question>(options);

        return questions.FirstOrDefault() ?? throw new Exception($"Could not find question with id {id}");
    }
}
