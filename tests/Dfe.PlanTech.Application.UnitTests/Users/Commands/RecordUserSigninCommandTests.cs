using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class RecordUserSigninCommandTests
    {
        public Mock<IPlanTechDbContext> mockDb = new Mock<IPlanTechDbContext>();
        public Mock<IGetUserIdQuery> mockUserQuery = new Mock<IGetUserIdQuery>();
        public Mock<ICreateUserCommand> mockCreateUserCommand = new Mock<ICreateUserCommand>();
        public Mock<IGetEstablishmentIdQuery> mockGetEstablishmentIdQuery = new();
        private Mock<ICreateEstablishmentCommand> mockCreateEstablishmentCommand = new();

        public RecordUserSignInCommand CreateStrut()
        {
            return new RecordUserSignInCommand(mockDb.Object, mockCreateEstablishmentCommand.Object, mockCreateUserCommand.Object, mockGetEstablishmentIdQuery.Object, mockUserQuery.Object);
        }


        [Fact]
        public async Task RecordSignInForExistingUser_UpdatesSignInDetailsAnd_ReturnsId()
        {
            //Arrange
            Domain.SignIn.Models.SignIn? createdSignIn = null;
            int signInId = 1;

            var guid = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                DfeSignInRef = guid
            };

            var strut = CreateStrut();
            mockUserQuery.Setup(x => x.GetUserId(It.IsAny<string>())).ReturnsAsync(1);
            mockDb.Setup(x => x.GetUserBy(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(user);
            mockDb.Setup(x => x.AddSignIn(It.IsAny<Domain.SignIn.Models.SignIn>()))
                    .Callback((Domain.SignIn.Models.SignIn signIn) => createdSignIn = signIn);
            mockDb.Setup(x => x.SaveChangesAsync()).Callback(() =>
            {
                if (createdSignIn != null)
                {
                    createdSignIn.Id = signInId;
                }
            });
            var recordUserSignInDto = new RecordUserSignInDto
            {
                DfeSignInRef = guid,
                Organisation = new Organisation()
                {
                    Ukprn = "ukprn",
                    Type = new IdWithName()
                    {
                        Name = "School",
                        Id = "Id"
                    }
                }
            };

            //Act
            var result = await strut.RecordSignIn(recordUserSignInDto);

            //Assert
            Assert.Equal(signInId, result);
            mockDb.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(2402)]
        public async Task RecordSignInForNewUser_AddsUser_UpdatesSignInDetailsAnd_ReturnsId(int userId)
        {
            User? createdUser = null;
            Domain.SignIn.Models.SignIn? createdSignIn = null;
            int signInId = 1;

            mockDb.Setup(x => x.GetUserBy(It.IsAny<Expression<Func<User, bool>>>()))
                    .ReturnsAsync(() => null);

            mockDb.Setup(db => db.AddUser(It.IsAny<User>()))
                    .Callback((User user) => createdUser = user);

            mockDb.Setup(db => db.AddSignIn(It.IsAny<Domain.SignIn.Models.SignIn>()))
                    .Callback((Domain.SignIn.Models.SignIn signIn) => createdSignIn = signIn);

            mockDb.Setup(db => db.SaveChangesAsync()).Callback(() =>
            {
                if (createdUser != null)
                {
                    createdUser.Id = userId;
                }

                if (createdSignIn != null)
                {
                    createdSignIn.Id = signInId;
                }
            });

            var createUserCommand = new CreateUserCommand(mockDb.Object);

            var recordUserSignInDto = new RecordUserSignInDto
            {
                DfeSignInRef = new Guid().ToString(),
                Organisation = new Organisation()
                {
                    Urn = "urn",
                    Type = new IdWithName()
                    {
                        Name = "School",
                        Id = "Id"
                    }
                }
            };

            var recordUserSignInCommand = new RecordUserSignInCommand(mockDb.Object, mockCreateEstablishmentCommand.Object, createUserCommand, mockGetEstablishmentIdQuery.Object, mockUserQuery.Object);
            var result = await recordUserSignInCommand.RecordSignIn(recordUserSignInDto);

            mockCreateUserCommand.Verify();

            Assert.Equal(userId, createdUser?.Id);

            Assert.Equal(signInId, createdSignIn?.Id);
            mockDb.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task RecordSignIn_ThrowsException_WhenUserIsNull()
        {
            //Arrange
            var strut = CreateStrut();

            var recordUserSignInDto = new RecordUserSignInDto
            {
                DfeSignInRef = null!,
                Organisation = new Organisation()
                {
                    Urn = "Urn",
                    Type = new IdWithName()
                    {
                        Name = "School",
                        Id = "Id"
                    }
                }
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => strut.RecordSignIn(recordUserSignInDto));
            mockDb.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}
