using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.Users.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class CreateUserCommandTests
    {
        public IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();

        public CreateUserCommand CreateStrut()
        {
            return new CreateUserCommand(Db);
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

            Db.When(x => x.AddUser(Arg.Any<User>()))
                    .Do((callInfo) =>
                    {
                        User user = (User)callInfo[0];
                        createdUser = user;
                    });

            Db.When(x => x.SaveChangesAsync())
                .Do(x =>
                {
                    if (createdUser != null)
                    {
                        createdUser.Id = expectedUserId;
                    }
                });

            var dfeSignInRef = Guid.NewGuid().ToString();

            //Act
            var result = await strut.CreateUser(dfeSignInRef);

            //Assert
            Assert.Equal(expectedUserId, result);
            Assert.NotNull(createdUser);
        }
    }
}
