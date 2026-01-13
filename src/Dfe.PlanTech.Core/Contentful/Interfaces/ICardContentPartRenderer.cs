using System.Text;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

public interface ICardContentPartRenderer
{
    public StringBuilder AddHtml(ComponentCardEntry? content, StringBuilder stringBuilder);
}
