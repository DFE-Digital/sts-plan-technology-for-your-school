using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Users.Queries
{
    public class GetUserIdQueryTests
    {
        public Mock<IPlanTechDbContext> mockDb = new Mock<IPlanTechDbContext>();

        public GetUserIdQuery CreateStrut()
        {
            return new GetUserIdQuery(mockDb.Object);
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
            mockDb.Setup(x => x.GetUserBy(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(returnedUser);

            //Act
            var result = await strut.GetUserId(dfeSignInRef);

            //Assert
            Assert.Equal(expectedUserId, result);
        }
    }
}
