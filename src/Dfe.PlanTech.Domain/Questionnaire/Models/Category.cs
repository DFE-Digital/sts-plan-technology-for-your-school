using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Category : ContentComponent, ICategoryComponent
{
    public Header Header { get; set; } = null!;
    public List<Section> Sections { get; set; } = new();
    public IList<SectionStatusDto> SectionStatuses { get; set; } = new List<SectionStatusDto>();


    public int Completed { get; set; }
    public bool RetrievalError { get; set; }
}