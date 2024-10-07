using Bogus;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class ReferencedEntityGeneratorHelper<TEntity>(List<TEntity> allEntities)
where TEntity : ContentComponent
{
    private readonly List<TEntity> _remainingEntities = [.. allEntities];

    public TEntity? GetEntity(Faker faker)
    {
        if (_remainingEntities.Count == 0)
        {
            return null;
        }

        int entityToPickIndex = GetEntityCountToPick(faker, 0, _remainingEntities.Count - 1);

        var entity = _remainingEntities[entityToPickIndex];

        _remainingEntities.RemoveAt(entityToPickIndex);

        return entity;
    }

    public List<TEntity> GetEntities(Faker faker, int min, int max)
    {
        if (_remainingEntities.Count == 0)
        {
            return [];
        }

        var amountOfEntitiesToPick = Math.Min(_remainingEntities.Count, Math.Max(0, GetEntityCountToPick(faker, min, max)));

        var sliceLength = GetSliceLength(amountOfEntitiesToPick);

        var startIndex = faker.Random.Int(0, sliceLength);

        return GetEntitiesAndRemoveFromList(amountOfEntitiesToPick, startIndex);
    }

    private List<TEntity> GetEntitiesAndRemoveFromList(int amountOfEntitiesToPick, int startIndex)
    {
        var entities = _remainingEntities[startIndex..(startIndex + amountOfEntitiesToPick)];

        _remainingEntities.RemoveAll(answer => entities.Exists(pickedAnswer => pickedAnswer.Sys.Id == answer.Sys.Id));

        return entities;
    }

    private static int GetEntityCountToPick(Faker faker, int min, int max) => faker.Random.Int(min, max);

    private int GetSliceLength(int amountToPick) => Math.Min(_remainingEntities.Count - 1, Math.Max(0, _remainingEntities.Count - amountToPick - 1));
}
