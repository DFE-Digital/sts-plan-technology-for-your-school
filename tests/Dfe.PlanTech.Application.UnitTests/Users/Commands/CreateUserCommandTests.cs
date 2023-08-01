using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class CreateUserCommandTests
    {
        public Mock<IPlanTechDbContext> mockDb = new Mock<IPlanTechDbContext>();

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
            User? createdUser = null;

            mockDb.Setup(x => x.AddUser(It.IsAny<User>()))
                    .Callback((User user) => createdUser = user);

            mockDb.Setup(x => x.SaveChangesAsync()).Callback(() =>
            {
                if (createdUser != null)
                {
                    createdUser.Id = expectedUserId;
                }
            });


            var recordUserSignInDto = new RecordUserSignInDto
            {
                DfeSignInRef = new Guid().ToString(),
                Organisation = new Organisation()
                {
                    Ukprn = "Ukprn",
                    Type = new IdWithName()
                    {
                        Name = "School",
                        Id = "Id"
                    }
        } };

            //Act
            var result = await strut.CreateUser(recordUserSignInDto);

            //Assert
            Assert.Equal(expectedUserId, result);
        }
    }
}
