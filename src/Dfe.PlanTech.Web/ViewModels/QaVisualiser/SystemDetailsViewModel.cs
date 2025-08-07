using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Web.ViewModels.QaVisualiser
{
    public class SystemDetailsViewModel
    {
        public string Id { get; init; } = null!;

        public SystemDetailsViewModel(CmsEntrySystemDetailsDto systemDetailsDto)
        {
            if (systemDetailsDto?.Id is null)
            {
                throw new InvalidDataException($"{nameof(systemDetailsDto)} has no ID");
            }

            Id = systemDetailsDto.Id;
        }
    }
}
