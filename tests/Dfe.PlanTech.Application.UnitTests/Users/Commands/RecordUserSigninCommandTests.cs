using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.ReturnsExtensions;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class RecordUserSigninCommandTests
    {
        public IPlanTechDbContext mockDb = Substitute.For<IPlanTechDbContext>();
        public IGetUserIdQuery mockUserQuery = Substitute.For<IGetUserIdQuery> ();
        public ICreateUserCommand mockCreateUserCommand = Substitute.For<ICreateUserCommand>();
        public IGetEstablishmentIdQuery mockGetEstablishmentIdQuery = Substitute.For<IGetEstablishmentIdQuery>();
        private ICreateEstablishmentCommand mockCreateEstablishmentCommand = Substitute.For<ICreateEstablishmentCommand>();

        public RecordUserSignInCommand CreateStrut()
        {
            return new RecordUserSignInCommand(mockDb, mockCreateEstablishmentCommand, mockCreateUserCommand, mockGetEstablishmentIdQuery, mockUserQuery);
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
            mockUserQuery.GetUserId(Arg.Any<string>()).Returns(1);
            mockDb.GetUserBy(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            mockDb.When(x => x.AddSignIn(Arg.Any<Domain.SignIn.Models.SignIn>())).Do(callInfo => 
            {
                createdSignIn = (Domain.SignIn.Models.SignIn)callInfo[0];
            });
            mockDb.When(x => x.SaveChangesAsync())
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
            Assert.Equal(signInId, result);
            await mockDb.Received(1).SaveChangesAsync();
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

            mockDb.GetUserBy(Arg.Any<Expression<Func<User, bool>>>()).ReturnsNull();

            mockDb.When(x => x.AddUser(Arg.Any<User>())).Do((callInfo) => 
            {
                User user = (User)callInfo[0];
                createdUser = user;
            });

            mockDb.When(x => x.AddSignIn(Arg.Any<Domain.SignIn.Models.SignIn>())).Do(callInfo =>
            {
                createdSignIn = (Domain.SignIn.Models.SignIn)callInfo[0];
            });

            mockDb.SaveChangesAsync().Returns((callInfo) =>
            {
                if (createdUser != null)
                {
                   return createdUser.Id = userId;
                }

                if (createdSignIn != null)
                {
                    return createdSignIn.Id = signInId;
                }
                return callInfo[0];
            });

            var createUserCommand = new CreateUserCommand(mockDb);

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

            var recordUserSignInCommand = new RecordUserSignInCommand(mockDb, mockCreateEstablishmentCommand, createUserCommand, mockGetEstablishmentIdQuery, mockUserQuery);
            var result = await recordUserSignInCommand.RecordSignIn(recordUserSignInDto);

            Assert.Equal(userId, createdUser?.Id);

            Assert.Equal(signInId, createdSignIn?.Id);
            await mockDb.Received(2).SaveChangesAsync();
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
            await mockDb.Received(0).SaveChangesAsync();
        }
    }
}
