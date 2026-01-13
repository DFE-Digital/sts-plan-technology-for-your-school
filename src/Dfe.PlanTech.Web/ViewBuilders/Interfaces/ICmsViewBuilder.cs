using Dfe.PlanTech.Web.ViewModels.QaVisualiser;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface ICmsViewBuilder
    {
        Task<IEnumerable<SectionViewModel>> GetAllSectionsAsync();
        Task<IActionResult> GetChunks(Controller controller, int? page);
    }
}
