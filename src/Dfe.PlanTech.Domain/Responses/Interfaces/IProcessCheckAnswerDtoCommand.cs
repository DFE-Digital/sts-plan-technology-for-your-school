using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Responses.Interfaces;

public interface IProcessCheckAnswerDtoCommand
{
    public Task<CheckAnswerDto?> GetCheckAnswerDtoForSection(int establishmentId, Section section, CancellationToken cancellationToken = default);
}
