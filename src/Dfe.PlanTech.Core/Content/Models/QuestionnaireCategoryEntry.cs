using Contentful.Core.Models;
using Dfe.PlanTech.Core.Content.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class QuestionnaireCategoryEntry : Entry<ContentComponent>
{
    public string InternalName { get; set; } = "";
    public ComponentHeaderEntry Header { get; set; } = null!;
    public List<ContentComponent> Content { get; set; } = null!;
    public List<Section> Sections { get; set; } = [];
    public int Completed { get; set; }
    public bool RetrievalError { get; set; }

    public CmsCategoryDto AsDto => new(this);
}
