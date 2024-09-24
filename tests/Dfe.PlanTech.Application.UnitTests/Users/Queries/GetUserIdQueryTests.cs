using System.Linq.Expressions;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Users.Queries
{
    public class GetUserIdQueryTests
    {
        public IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();

        private readonly User[] Users = new User[]{
            new() { DfeSignInRef = "One", Id = 1 },
            new() { DfeSignInRef = "Two", Id = 2 },
            new() { DfeSignInRef = "Three", Id = 3 },
        };

        public GetUserIdQuery CreateStrut()
        {
            return new GetUserIdQuery(Db);
        }

        [Theory]
        [InlineData("One", 1)]
        [InlineData("Two", 2)]
        [InlineData("Three", 3)]
        public async Task GetUserId_Returns_TheCorrectUserId(string userRef, int expectedUserId)
        {
            //Arrange
            var strut = CreateStrut();

            Db.GetUserBy(Arg.Any<Expression<Func<User, bool>>>())
                .Returns((callinfo) =>
                {
                    var expression = callinfo.ArgAt<Expression<Func<User, bool>>>(0);

                    return Users.AsQueryable().FirstOrDefault(expression);
                });

            //Act
            var result = await strut.GetUserId(userRef);

            //Assert
            Assert.Equal(expectedUserId, result);
        }
    }
}
