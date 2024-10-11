using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Extensions;

namespace Dfe.PlanTech.Application.UnitTests.TestHelpers;

public static class DatabaseTestHelpers
{
    public static void MockDatabaseCollection<T>(this IDatabaseHelper<ICmsDbContext> databaseHelper,
        List<T> sourceEntities)
        where T : class
    {
        var queryable = sourceEntities.AsQueryable();
        databaseHelper.Database.ReturnsForAll(queryable);
        databaseHelper.GetQueryableForEntityExcludingAutoIncludesAndFilters<T>().Returns(queryable);
        databaseHelper.GetQueryableForEntityExcludingAutoIncludesAndFilters(Arg.Any<T>()).Returns(queryable);
        databaseHelper.When(dbHelper => dbHelper.Remove(Arg.Any<T>())).Do(callinfo => RemoveEntity(callinfo, sourceEntities));
        databaseHelper.When(dbHelper => dbHelper.Add(Arg.Any<T>())).Do(callinfo => AddEntity(callinfo, sourceEntities));
        databaseHelper.When(dbHelper => dbHelper.Update(Arg.Any<T>())).Do(callinfo =>
        {
            UpdateEntity(callinfo, sourceEntities);
            return;
        });
        databaseHelper.Database.MockFirstOrDefaultAsync<T>();
        databaseHelper.Database.MockListAsync<T>();
    }

    private static void RemoveEntity<T>(CallInfo callInfo, List<T> sourceEntities) where T : class
    {
        var entity = GetEntityArg<T>(callInfo);

        var index = sourceEntities.FindIndex(existing => EntitiesMatch(entity, existing));
        if (index == -1)
        {
            throw new Exception("Couldn't find matching entity");
        }

        sourceEntities.Remove(entity);
    }

    private static bool EntitiesMatch<T>(T first, object second) where T : class
    {
        if (first == second || first.Equals(second))
        {
            return true;
        }

        if (second.GetType() != typeof(T))
        {
            return false;
        }

        if (second is ContentComponentDbEntity existingContent && first is ContentComponentDbEntity otherContent)
        {
            return existingContent.Id == otherContent.Id;
        }

        return false;
    }

    private static void UpdateEntity<T>(CallInfo callInfo, List<T> sourceEntities) where T : class
    {
        var entity = GetEntityArg<T>(callInfo);

        var index = sourceEntities.FindIndex(existing => EntitiesMatch(entity, existing));

        if (index == -1)
        {
            throw new Exception("Couldn't find matching entity");
        }

        sourceEntities[index] = entity;
    }


    private static void AddEntity<T>(CallInfo callInfo, List<T> sourceEntities) where T : class
    {
        var entity = GetEntityArg<T>(callInfo);

        sourceEntities.Add(entity);
    }


    private static T GetEntityArg<T>(CallInfo callInfo) where T : class
    {
        var entity = callInfo.ArgAt<T>(0);

        if (entity == null)
        {
            throw new Exception($"Entity is null");
        }

        return entity;
    }


    private static void MockFirstOrDefaultAsync<T>(this ICmsDbContext db)
    {
        db.FirstOrDefaultCachedAsync(Arg.Any<IQueryable<T>>(), Arg.Any<CancellationToken>())
            .Returns((callInfo) =>
            {
                var queryable = callInfo.Arg<IQueryable<T>>();
                var matching = queryable.FirstOrDefault();
                return Task.FromResult(matching);
            });
    }

    private static void MockListAsync<T>(this ICmsDbContext db)
    {
        db.ToListCachedAsync(Arg.Any<IQueryable<T>>(), Arg.Any<CancellationToken>())
            .Returns((callInfo) =>
            {
                var queryable = callInfo.Arg<IQueryable<T>>();
                return Task.FromResult(queryable.ToList());
            });
    }
}
