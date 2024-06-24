using System.Reflection;
using Bogus;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class BaseGenerator<TContentComponent> : Faker<TContentComponent>
where TContentComponent : ContentComponent
{
    protected readonly PropertyInfo IdPropertyInfo = typeof(SystemDetails).GetProperty("Id")!;

    public BaseGenerator()
    {
        this.GenerateSys();
    }

    public TContentComponent CopyWithDifferentValues(TContentComponent contentComponent)
    {
        var newEntity = Generate();

        IdPropertyInfo.SetValue(newEntity.Sys, Convert.ChangeType(contentComponent.Sys.Id, IdPropertyInfo.PropertyType), null);

        return newEntity;
    }

    public IEnumerable<UpdatedEntity<TContentComponent>> CopyWithDifferentValues(IEnumerable<TContentComponent> entities)
     => entities.Select(originalEntity => new UpdatedEntity<TContentComponent>(originalEntity, CopyWithDifferentValues(originalEntity)));

    protected TContent CreateCopyOfEntityWithJustSys<TContent>(TContent content)
      where TContent : ContentComponent, new()
      => new() { Sys = content.Sys };
}