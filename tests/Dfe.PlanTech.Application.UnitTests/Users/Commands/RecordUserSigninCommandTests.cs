using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class RecordUserSigninCommandTests
    {
        public Mock<IUsersDbContext> mockDb = new Mock<IUsersDbContext>();
        public Mock<IGetUserIdQuery> mockUserQuery = new Mock<IGetUserIdQuery>();
        public Mock<ICreateUserCommand> mockCreateUserCommand= new Mock<ICreateUserCommand>();

        public RecordUserSignInCommand CreateStrut()
        {
            return new RecordUserSignInCommand(mockDb.Object, mockCreateUserCommand.Object);
        }


        [Fact]
        public async Task RecordSignInForNewUser_AddsUser_UpdatesSignInDetailsAnd_ReturnsId()
        {
            //Arrange
            var guid = Guid.NewGuid().ToString();
            var user = new User { 
                Id= 1,
                DfeSignInRef = guid
            };
            var strut = CreateStrut();
            mockUserQuery.Setup(x => x.GetUserId(It.IsAny<string>())).ReturnsAsync(1);
            mockDb.Setup(x => x.GetUserBy(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            mockDb.Setup(x => x.AddSignIn(It.IsAny<Domain.SignIn.Models.SignIn>()));
            mockDb.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            var recordUserSignInDto = new RecordUserSignInDto { DfeSignInRef = guid };

            //Act
            var result = await strut.RecordSignIn(recordUserSignInDto);

            //Assert
            Assert.Equal(1, result);
            mockDb.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RecordSignInForExistingUser_UpdatesSignInDetailsAnd_ReturnsId()
        {
            //Arrange
            var strut = CreateStrut();
            var user = new User
            {
                Id = 1,
                DfeSignInRef = Guid.NewGuid().ToString(),
                DateCreated= DateTime.UtcNow,
            };
            mockUserQuery.Setup(x => x.GetUserId(It.IsAny<string>())).ReturnsAsync(1);
            mockDb.Setup(x => x.GetUserBy(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            mockDb.Setup(x => x.AddSignIn(It.IsAny<Domain.SignIn.Models.SignIn>()));
            mockDb.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            var recordUserSignInDto = new RecordUserSignInDto { DfeSignInRef = new Guid().ToString() };

            //Act
            var result = await strut.RecordSignIn(recordUserSignInDto);

            //Assert
            Assert.Equal(1, result);
            mockDb.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RecordSignIn_ThrowsException_WhenUserIsNull()
        {
            //Arrange
            var strut = CreateStrut();
            User user = null!;
            mockUserQuery.Setup(x => x.GetUserId(It.IsAny<string>())).ReturnsAsync(1);
            mockDb.Setup(x => x.GetUserBy(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            mockDb.Setup(x => x.AddSignIn(It.IsAny<Domain.SignIn.Models.SignIn>()));
            var recordUserSignInDto = new RecordUserSignInDto { DfeSignInRef = new Guid().ToString() };
            int result = 0;

            try
            {
                //Act
                result = await strut.RecordSignIn(recordUserSignInDto);
            }
            catch (ArgumentNullException ex)
            {
                //Assert
                Assert.Contains("User id cannot be null", ex.Message);
            }

            mockDb.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}
