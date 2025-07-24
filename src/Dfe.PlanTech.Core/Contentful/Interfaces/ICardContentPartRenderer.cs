using System.Text;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface ICardContentPartRenderer
{
    public StringBuilder AddHtml(CmsCardComponentDto? content, StringBuilder stringBuilder);
}
