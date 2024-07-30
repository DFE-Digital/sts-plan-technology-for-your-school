using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class ButtonsComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Value", "IsStartButton"], "Button")
{
    public override Task ValidateContent()
    {
        ValidateButtons(_dbEntities.OfType<ButtonDbEntity>().ToArray());
        return Task.CompletedTask;
    }

    public void ValidateButtons(ButtonDbEntity[] dbButtons)
    {
        foreach (var contentfulButton in _contentfulEntities)
        {
            ValidateButton(dbButtons, contentfulButton);
        }
    }

    private void ValidateButton(ButtonDbEntity[] dbButtons, JsonNode contentfulButton)
    {
        var databaseButton = TryRetrieveMatchingDbEntity(dbButtons, contentfulButton);
        if (databaseButton == null)
        {
            return;
        }

        ValidateProperties(contentfulButton, databaseButton);
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.Buttons;
    }
}
