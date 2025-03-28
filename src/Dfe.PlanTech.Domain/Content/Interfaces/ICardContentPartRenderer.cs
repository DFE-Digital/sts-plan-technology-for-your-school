using System.Text;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface ICardContentPartRenderer
{
    public StringBuilder AddHtml(CsCard? content, StringBuilder stringBuilder);
}
