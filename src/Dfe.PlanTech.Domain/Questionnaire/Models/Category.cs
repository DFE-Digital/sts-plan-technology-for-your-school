using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Category : ContentComponent, ICategoryComponent
{
    public string InternalName { get; set; } = "";
    public Header Header { get; set; } = null!;
    public List<Section> Sections { get; set; } = [];
    public IList<SectionStatusDto> SectionStatuses { get; set; } = [];
    public int Completed { get; set; }
    public bool RetrievalError { get; set; }
}
