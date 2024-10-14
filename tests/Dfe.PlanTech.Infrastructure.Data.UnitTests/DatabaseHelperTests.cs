using System.Reflection;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Data.UnitTests;

public class DatabaseHelperTests
{
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly IQueryCacher _queryCacher = Substitute.For<IQueryCacher>();
    private readonly CmsDbContext _mockDb = Substitute.For<CmsDbContext>();
    private readonly DatabaseHelper<ICmsDbContext> _databaseHelper;
    private readonly string[] _nonNullablePropertyNames = ["Id", "Archived", "Published", "Deleted", "Slug", "Text"];

    public DatabaseHelperTests()
    {
        _serviceProvider.GetService(typeof(IQueryCacher)).Returns(_queryCacher);
        _serviceProvider.GetService(typeof(CmsDbContext)).Returns(_mockDb);
        _serviceProvider.GetService(typeof(ICmsDbContext)).Returns(_mockDb);
        AddRealDbProperties();

        _databaseHelper = new DatabaseHelper<ICmsDbContext>(_serviceProvider);
    }

    private void AddRealDbProperties()
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<CmsDbContext>();
        dbContextOptionsBuilder.UseSqlServer("NotARealSqlServer");

        var services = new ServiceCollection();

        services.AddSingleton(new ContentfulOptions());
        services.AddSingleton(_queryCacher);
        dbContextOptionsBuilder.UseApplicationServiceProvider(services.BuildServiceProvider());
        var actualDbContext = new CmsDbContext(dbContextOptionsBuilder.Options);

        var mockChangeTracker = Substitute.For<ChangeTracker>(actualDbContext,
            //Complaint about these types being internal so open to frequent changes
            //However, kinda need to mock them
#pragma warning disable EF1001
            Substitute.For<IStateManager>(),
            Substitute.For<IChangeDetector>(),
#pragma warning restore EF1001
            actualDbContext.Model,
            Substitute.For<IEntityEntryGraphIterator>());
        _mockDb.ChangeTracker.Returns(mockChangeTracker);
        _mockDb.Model.Returns(actualDbContext.Model);
    }

    [Fact]
    public void ClearTracking_ShouldClearChangeTracker()
    {
        _databaseHelper.ClearTracking();
        _mockDb.ChangeTracker.Received(1).Clear();
    }

    [Fact]
    public void Add_ShouldCallAddOnDbContext()
    {
        var entity = new AnswerDbEntity();

        _databaseHelper.Add(entity);
        _mockDb.Received(1).Add(entity);
    }

    [Fact]
    public void Update_ShouldCallUpdateOnDbContext()
    {
        var entity = new AnswerDbEntity();

        _databaseHelper.Update(entity);

        _mockDb.Received(1).Update(entity);
    }

    [Fact]
    public void Remove_ShouldCallRemoveOnDbContext()
    {
        var entity = new AnswerDbEntity();

        _databaseHelper.Remove(entity);

        _mockDb.Received(1).Remove(entity);
    }

    [Fact]
    public void GetRequiredPropertiesForType_Should_Return_MatchingProperties()
    {
        var type = typeof(QuestionDbEntity);
        var properties = _databaseHelper.GetRequiredPropertiesForType(type).ToArray();
        Assert.NotEmpty(properties);
        Assert.Equal(_nonNullablePropertyNames.Length, properties.Length);
        Assert.True(properties.All(property => _nonNullablePropertyNames.Contains(property.Name)));
    }

    [Fact]
    public void GetDbSet_Returns_MatchingDbSet()
    {
        var methodInfo = _databaseHelper.GetType()
            .GetMethod("GetDbSet", BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes)!;

        var genericMethod = methodInfo.MakeGenericMethod(typeof(QuestionDbEntity));

        var dbSet = (DbSet<QuestionDbEntity>)genericMethod.Invoke(_databaseHelper, null)!;

        Assert.NotNull(dbSet);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldCallSaveChangesAsync()
    {
        await _databaseHelper.SaveChangesAsync(CancellationToken.None);

        await _mockDb.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetIQueryableForEntity_Success()
    {
        var result = _databaseHelper.GetIQueryableForEntity<QuestionDbEntity>();

        Assert.NotNull(result);
    }

    [Fact]
    public void GetDbContext_ThrowsException_When_ConcreteDbContext_NotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => new DatabaseHelper<IMockDbContext>(_serviceProvider));
    }
}

public interface IMockDbContext : IDbContext
{
}

public class MockDbContext : IMockDbContext
{
    public Task<T?> FirstOrDefaultCachedAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> ToListCachedAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
