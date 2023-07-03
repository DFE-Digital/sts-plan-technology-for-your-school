using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class RecordUserSigninCommandTests
    {
        public Mock<IUsersDbContext> mockDb = new Mock<IUsersDbContext>();
        public Mock<IGetUserIdQuery> mockUserQuery = new Mock<IGetUserIdQuery>();

        public RecordUserSignInCommand CreateStrut()
        {
            return new RecordUserSignInCommand(mockDb.Object);
        }


        [Fact]
        public async Task RecordSignInForNewUser_AddsUser_UpdatesSignInDetailsAnd_ReturnsId()
        {
            //Arrange
            var strut = CreateStrut();
            mockUserQuery.Setup(x => x.GetUserId(It.IsAny<string>())).ReturnsAsync(1);
            mockDb.Setup(x => x.AddSignIn(It.IsAny<Domain.SignIn.Models.SignIn>()));
            mockDb.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            var recordUserSignInDto = new RecordUserSignInDto { DfeSignInRef = new Guid().ToString() };

            //Act
            var result = await strut.RecordSignIn(recordUserSignInDto);

            //Assert
            Assert.Equal(1, result);
            mockDb.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
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
    }
}
