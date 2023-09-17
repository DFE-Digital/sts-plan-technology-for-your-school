using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Responses.Interfaces;

public interface IProcessCheckAnswerDtoCommand
{
  public Task<CheckAnswerDto?> GetCheckAnswerDtoForSectionId(int establishmentId, string sectionId, CancellationToken cancellationToken = default);
}
