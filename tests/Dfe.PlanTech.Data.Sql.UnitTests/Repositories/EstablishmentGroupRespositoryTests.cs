using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Data.Sql.UnitTests.Repositories;

public class EstablishmentGroupRepositoryTests
{
    private static PlanTechDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PlanTechDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PlanTechDbContext(options);
    }

    [Fact]
    public async Task GetLinkedEstablishmentsByGroupEstablishmentIdAsync_ReturnsLinkedEstablishments()
    {
        await using var db = CreateDbContext();

        db.Establishments.AddRange(
            new EstablishmentEntity
            {
                Id = 100,
                GroupUid = "500",
                EstablishmentRef = "GROUP-100",
                OrgName = "Test Academy Trust",
            },
            new EstablishmentEntity
            {
                Id = 1,
                EstablishmentRef = "URN-1",
                OrgName = "School One",
            },
            new EstablishmentEntity
            {
                Id = 2,
                EstablishmentRef = "URN-2",
                OrgName = "School Two",
            }
        );

        db.EstablishmentGroups.Add(
            new EstablishmentGroupEntity
            {
                Uid = "500",
            }
        );

        db.EstablishmentLinks.AddRange(
            new EstablishmentLinkEntity
            {
                GroupUid = "500",
                Urn = "URN-1",
            },
            new EstablishmentLinkEntity
            {
                GroupUid = "500",
                Urn = "URN-2",
            }
        );

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = new EstablishmentGroupRepository(db);

        var result =
            await sut.GetLinkedEstablishmentsByGroupEstablishmentIdAsync(100);

        Assert.Equal(2, result.Count);

        Assert.Collection(
            result.OrderBy(e => e.Id),
            establishment =>
            {
                Assert.Equal(1, establishment.Id);
                Assert.Equal("School One", establishment.OrgName);
                Assert.Equal("URN-1", establishment.EstablishmentRef);
            },
            establishment =>
            {
                Assert.Equal(2, establishment.Id);
                Assert.Equal("School Two", establishment.OrgName);
                Assert.Equal("URN-2", establishment.EstablishmentRef);
            }
        );
    }

    [Fact]
    public async Task GetLinkedEstablishmentsByGroupEstablishmentIdAsync_DoesNotReturnEstablishmentsFromOtherGroups()
    {
        await using var db = CreateDbContext();

        db.Establishments.AddRange(
            new EstablishmentEntity
            {
                Id = 100,
                GroupUid = "500",
                EstablishmentRef = "GROUP-100",
                OrgName = "Test Academy Trust",
            },
            new EstablishmentEntity
            {
                Id = 200,
                GroupUid = "600",
                EstablishmentRef = "GROUP-200",
                OrgName = "Another Academy Trust",
            },
            new EstablishmentEntity
            {
                Id = 1,
                EstablishmentRef = "URN-1",
                OrgName = "School One",
            },
            new EstablishmentEntity
            {
                Id = 2,
                EstablishmentRef = "URN-2",
                OrgName = "School Two",
            }
        );

        db.EstablishmentGroups.AddRange(
            new EstablishmentGroupEntity
            {
                Uid = "500",
            },
            new EstablishmentGroupEntity
            {
                Uid = "600",
            }
        );

        db.EstablishmentLinks.AddRange(
            new EstablishmentLinkEntity
            {
                GroupUid = "500",
                Urn = "URN-1",
            },
            new EstablishmentLinkEntity
            {
                GroupUid = "600",
                Urn = "URN-2",
            }
        );

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = new EstablishmentGroupRepository(db);

        var result =
            await sut.GetLinkedEstablishmentsByGroupEstablishmentIdAsync(100);

        var establishment = Assert.Single(result);

        Assert.Equal(1, establishment.Id);
        Assert.Equal("School One", establishment.OrgName);
    }

    [Fact]
    public async Task GetLinkedEstablishmentsByGroupEstablishmentIdAsync_WhenNoLinkedEstablishments_ReturnsEmptyList()
    {
        await using var db = CreateDbContext();

        db.Establishments.Add(
            new EstablishmentEntity
            {
                Id = 100,
                GroupUid = "500",
                EstablishmentRef = "GROUP-100",
                OrgName = "Test Academy Trust",
            }
        );

        db.EstablishmentGroups.Add(
            new EstablishmentGroupEntity
            {
                Uid = "500",
            }
        );

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var sut = new EstablishmentGroupRepository(db);

        var result =
            await sut.GetLinkedEstablishmentsByGroupEstablishmentIdAsync(100);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLinkedEstablishmentsByGroupEstablishmentIdAsync_WhenGroupEstablishmentDoesNotExist_ReturnsEmptyList()
    {
        await using var db = CreateDbContext();

        var sut = new EstablishmentGroupRepository(db);

        var result =
            await sut.GetLinkedEstablishmentsByGroupEstablishmentIdAsync(999);

        Assert.Empty(result);
    }
}
