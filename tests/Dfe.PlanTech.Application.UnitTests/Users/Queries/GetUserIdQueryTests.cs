using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;
using NSubstitute;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Users.Queries
{
    public class GetUserIdQueryTests
    {
        public IPlanTechDbContext mockDb = Substitute.For<IPlanTechDbContext>();

        public GetUserIdQuery CreateStrut()
        {
            return new GetUserIdQuery(mockDb);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(30)]
        public async Task GetUserId_Returns_TheCorrectUserId(int expectedUserId)
        {
            //Arrange
            var strut = CreateStrut();
            var dfeSignInRef = new Guid().ToString();
            var returnedUser = new User { DfeSignInRef = dfeSignInRef, Id = expectedUserId };
            mockDb.GetUserBy(Arg.Any<Expression<Func<User, bool>>>()).Returns(returnedUser);

            //Act
            var result = await strut.GetUserId(dfeSignInRef);

            //Assert
            Assert.Equal(expectedUserId, result);
        }
    }
}
