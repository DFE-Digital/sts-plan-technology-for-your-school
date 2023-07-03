using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.Users.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class RecordUserSigninCommandTests
    {
        public Mock<IUsersDbContext> mockDb = new Mock<IUsersDbContext>();

        public RecordUserSignInCommand CreateStrut()
        {
            return new RecordUserSignInCommand(mockDb.Object);
        }


        [Fact]
        public async Task RecordSignIn_UpdatesSignInDetailsAnd_ReturnsId(int expectedUserId)
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
