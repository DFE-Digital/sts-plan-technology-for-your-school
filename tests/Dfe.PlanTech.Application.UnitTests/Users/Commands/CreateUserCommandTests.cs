using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class CreateUserCommandTests
    {
        public IPlanTechDbContext mockDb = Substitute.For<IPlanTechDbContext>();

        public CreateUserCommand CreateStrut()
        {
            return new CreateUserCommand(mockDb);
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

            mockDb.When(x => x.AddUser(It.IsAny<User>()))
                    .Do((callInfo) => {
                    User user = (User)callInfo[0];
                        createdUser = user;
                    });

            mockDb.When(x => x.SaveChangesAsync())
                .Do(x =>
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
                }
            };

            //Act
            var result = await strut.CreateUser(recordUserSignInDto);

            //Assert
            Assert.Equal(expectedUserId, result);
        }
    }
}
