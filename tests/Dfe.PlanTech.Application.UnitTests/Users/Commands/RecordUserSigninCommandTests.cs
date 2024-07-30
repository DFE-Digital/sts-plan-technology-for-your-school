using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class RecordUserSigninCommandTests
    {
        public IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();
        public IGetUserIdQuery UserQuery = Substitute.For<IGetUserIdQuery>();
        public ICreateUserCommand CreateUserCommand = Substitute.For<ICreateUserCommand>();
        public IGetEstablishmentIdQuery GetEstablishmentIdQuery = Substitute.For<IGetEstablishmentIdQuery>();
        private readonly ICreateEstablishmentCommand CreateEstablishmentCommand = Substitute.For<ICreateEstablishmentCommand>();

        public RecordUserSignInCommand CreateStrut()
        {
            return new RecordUserSignInCommand(Db, CreateEstablishmentCommand, CreateUserCommand, GetEstablishmentIdQuery, UserQuery);
        }


        [Fact]
        public async Task RecordSignInForExistingUser_UpdatesSignInDetailsAnd_ReturnsId()
        {
            //Arrange
            SignIn? createdSignIn = null;
            int signInId = 1;

            var guid = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                DfeSignInRef = guid
            };

            var strut = CreateStrut();
            UserQuery.GetUserId(Arg.Any<string>()).Returns(1);
            Db.GetUserBy(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            Db.When(x => x.AddSignIn(Arg.Any<SignIn>())).Do(callInfo =>
            {
                createdSignIn = (SignIn)callInfo[0];
            });
            Db.When(x => x.SaveChangesAsync())
                .Do(x =>
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
            Assert.Equal(signInId, result.Id);
            await Db.Received(1).SaveChangesAsync();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(2402)]
        public async Task RecordSignInForNewUser_AddsUser_UpdatesSignInDetailsAnd_ReturnsId(int userId)
        {
            User? createdUser = null;
            SignIn? createdSignIn = null;
            int signInId = 1;

            Db.GetUserBy(Arg.Any<Expression<Func<User, bool>>>()).ReturnsNull();

            Db.When(x => x.AddUser(Arg.Any<User>())).Do((callInfo) =>
            {
                User user = (User)callInfo[0];
                createdUser = user;
            });

            Db.When(x => x.AddSignIn(Arg.Any<SignIn>())).Do((callInfo) =>
            {
                createdSignIn = (SignIn)callInfo[0];
            });

            Db.SaveChangesAsync().Returns((callInfo) =>
            {
                if (createdUser != null)
                {
                    createdUser.Id = userId;
                }

                if (createdSignIn != null)
                {
                    createdSignIn.Id = signInId;
                }
                return 0;
            });

            var createUserCommand = new CreateUserCommand(Db);

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

            var recordUserSignInCommand = new RecordUserSignInCommand(Db, CreateEstablishmentCommand, createUserCommand, GetEstablishmentIdQuery, UserQuery);
            var result = await recordUserSignInCommand.RecordSignIn(recordUserSignInDto);

            Assert.Equal(userId, createdUser?.Id);
            await Db.Received(2).SaveChangesAsync();
            Assert.Equal(signInId, createdSignIn?.Id);

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
            await Db.Received(0).SaveChangesAsync();
        }
    }
}
