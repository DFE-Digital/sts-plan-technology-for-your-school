using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class SystemDetailsViewModel
    {
        public string Id { get; init; } = null!;

        public SystemDetailsViewModel(SystemDetails systemDetailsDto)
        {
            if (systemDetailsDto?.Id is null)
            {
                throw new InvalidDataException($"{nameof(systemDetailsDto)} has no ID");
            }

            Id = systemDetailsDto.Id;
        }
    }
}
