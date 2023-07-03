using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class CreateUserCommandTests
    {
        public Mock<IUsersDbContext> mockDb = new Mock<IUsersDbContext>();

        public CreateUserCommand CreateStrut() 
        { 
            return new CreateUserCommand(mockDb.Object);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(30)]
        public async Task CreateUserReturnsIdOfNewlyCreatedUser(int expectedUserId) 
        {
            //Arrange
            var strut = CreateStrut();
            mockDb.Setup(x => x.AddUser(It.IsAny<User>()));
            mockDb.Setup(x => x.SaveChangesAsync()).ReturnsAsync(expectedUserId);
            var recordUserSignInDto = new RecordUserSignInDto { DfeSignInRef = new Guid().ToString() };

            //Act
            var result = await strut.CreateUser(recordUserSignInDto);

            //Assert
            Assert.Equal(expectedUserId, result);
        }
    }
}
