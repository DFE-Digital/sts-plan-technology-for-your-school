using Dfe.PlanTech.Application.Groups.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;

namespace Dfe.PlanTech.Application.UnitTests.Groups.Queries
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NSubstitute;
    using Xunit;

    public class GroupReadActivityServiceTests
    {
        private readonly IPlanTechDbContext _db;
        private readonly GetGroupSelectionQuery _service;

        public GroupReadActivityServiceTests()
        {
            _db = Substitute.For<IPlanTechDbContext>();
            _service = new GetGroupSelectionQuery(_db);
        }

        [Fact]
        public async Task GetLatestSelectedGroupSchool_ReturnsLatestEntry_WhenEntriesExist()
        {
            int userId = 1;
            int userEstablishmentId = 10;
            var activities = new List<GroupReadActivity>
            {
                new ()
                {
                    UserId = userId,
                    UserEstablishmentId = userEstablishmentId,
                    SelectedEstablishmentId = 99,
                    SelectedEstablishmentName = "New School",
                    DateSelected = new DateTime(2023, 1, 1)
                },
                new ()
                {
                    UserId = userId,
                    UserEstablishmentId = userEstablishmentId,
                    SelectedEstablishmentId = 100,
                    SelectedEstablishmentName = "Old School",
                    DateSelected = new DateTime(2023, 1, 1)
                },
                new ()
                {
                    UserId = userId,
                    UserEstablishmentId = userEstablishmentId,
                    SelectedEstablishmentId = 101,
                    SelectedEstablishmentName = "New School",
                    DateSelected = new DateTime(2024, 1, 1)
                }
            }.AsQueryable();

            _db.GetGroupReadActivities.Returns(activities);
            _db.FirstOrDefaultAsync(Arg.Any<IQueryable<GroupReadActivityDto>>(), Arg.Any<CancellationToken>())
                .Returns(ci =>
                {
                    var query = ci.Arg<IQueryable<GroupReadActivityDto>>();
                    return Task.FromResult(query.FirstOrDefault());
                });

            var result =
                await _service.GetLatestSelectedGroupSchool(userId, userEstablishmentId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(101, result.SelectedEstablishmentId);
            Assert.Equal("New School", result.SelectedEstablishmentName);
        }

        [Fact]
        public async Task GetLatestSelectedGroupSchool_ReturnsNull_WhenNoMatchingEntries()
        {
            _db.GetGroupReadActivities.Returns(new List<GroupReadActivity>().AsQueryable());

            _db.FirstOrDefaultAsync(Arg.Any<IQueryable<GroupReadActivityDto>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<GroupReadActivityDto?>(null));

            var result = await _service.GetLatestSelectedGroupSchool(1, 1, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestSelectedGroupSchool_IgnoresEntriesForOtherUsersOrEstablishments()
        {
            int userId = 1;
            int userEstablishmentId = 10;

            var activities = new List<GroupReadActivity>
            {
                new GroupReadActivity
                {
                    UserId = 2, // different user
                    UserEstablishmentId = userEstablishmentId,
                    SelectedEstablishmentId = 300,
                    SelectedEstablishmentName = "Wrong User School",
                    DateSelected = new DateTime(2024, 1, 1)
                },
                new GroupReadActivity
                {
                    UserId = userId,
                    UserEstablishmentId = 99, // different establishment
                    SelectedEstablishmentId = 301,
                    SelectedEstablishmentName = "Wrong Establishment",
                    DateSelected = new DateTime(2024, 1, 2)
                }
            }.AsQueryable();

            _db.GetGroupReadActivities.Returns(activities);
            _db.FirstOrDefaultAsync(Arg.Any<IQueryable<GroupReadActivityDto>>(), Arg.Any<CancellationToken>())
                .Returns(ci =>
                {
                    var query = ci.Arg<IQueryable<GroupReadActivityDto>>();
                    return Task.FromResult(query.FirstOrDefault());
                });

            var result =
                await _service.GetLatestSelectedGroupSchool(userId, userEstablishmentId, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestSelectedGroupSchool_ReturnsFirstEntry_WhenDatesAreSame()
        {
            int userId = 1;
            int userEstablishmentId = 10;
            var sameDate = new DateTime(2024, 1, 1);

            var activities = new List<GroupReadActivity>
            {
                new GroupReadActivity
                {
                    UserId = userId,
                    UserEstablishmentId = userEstablishmentId,
                    SelectedEstablishmentId = 200,
                    SelectedEstablishmentName = "First School",
                    DateSelected = sameDate
                },
                new GroupReadActivity
                {
                    UserId = userId,
                    UserEstablishmentId = userEstablishmentId,
                    SelectedEstablishmentId = 201,
                    SelectedEstablishmentName = "Second School",
                    DateSelected = sameDate
                }
            }.AsQueryable();

            _db.GetGroupReadActivities.Returns(activities);
            _db.FirstOrDefaultAsync(Arg.Any<IQueryable<GroupReadActivityDto>>(), Arg.Any<CancellationToken>())
                .Returns(ci =>
                {
                    var query = ci.Arg<IQueryable<GroupReadActivityDto>>();
                    return Task.FromResult(query.FirstOrDefault());
                });

            var result =
                await _service.GetLatestSelectedGroupSchool(userId, userEstablishmentId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(200, result.SelectedEstablishmentId);
        }

        [Fact]
        public async Task GetLatestSelectedGroupSchool_HandlesNullEstablishmentName()
        {
            int userId = 1;
            int userEstablishmentId = 10;

            var activities = new List<GroupReadActivity>
            {
                new GroupReadActivity
                {
                    UserId = userId,
                    UserEstablishmentId = userEstablishmentId,
                    SelectedEstablishmentId = 400,
                    SelectedEstablishmentName = "Establishment Name",
                    DateSelected = new DateTime(2024, 2, 1)
                }
            }.AsQueryable();

            _db.GetGroupReadActivities.Returns(activities);
            _db.FirstOrDefaultAsync(Arg.Any<IQueryable<GroupReadActivityDto>>(), Arg.Any<CancellationToken>())
                .Returns(ci =>
                {
                    var query = ci.Arg<IQueryable<GroupReadActivityDto>>();
                    return Task.FromResult(query.FirstOrDefault());
                });

            var result =
                await _service.GetLatestSelectedGroupSchool(userId, userEstablishmentId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(400, result.SelectedEstablishmentId);
            Assert.Null(result.SelectedEstablishmentName);
        }

    }
}
