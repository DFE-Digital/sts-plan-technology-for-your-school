using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.Establishments.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Users.Commands
{
    public class CreateEstablishmentCommandTests
    {
        public Mock<IPlanTechDbContext> mockDb = new Mock<IPlanTechDbContext>();

        public CreateEstablishmentCommand CreateStrut()
        {
            return new CreateEstablishmentCommand(mockDb.Object);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task CreateEstablishmentReturnsIdOfNewlyCreateEstablishmentWithUkprn(int expectedEstablishmentId)
        {
            //Arrange
            var strut = CreateStrut();
            mockDb.Setup(x => x.AddEstablishment(It.IsAny<Domain.Establishments.Models.Establishment>()));
            mockDb.Setup(x => x.SaveChangesAsync()).ReturnsAsync(expectedEstablishmentId);
            var establishmentDto = new EstablishmentDto() { Urn = null, Ukprn = new Guid().ToString()};

            //Act
            var result = await strut.CreateEstablishment(establishmentDto);

            //Assert
            Assert.Equal(expectedEstablishmentId, result);
        }
        
        [Theory]
        [InlineData(3)]
        [InlineData(300)]
        public async Task CreateEstablishmentReturnsIdOfNewlyCreateEstablishmentWithUrn(int expectedEstablishmentId)
        {
            //Arrange
            var strut = CreateStrut();
            mockDb.Setup(x => x.AddEstablishment(It.IsAny<Domain.Establishments.Models.Establishment>()));
            mockDb.Setup(x => x.SaveChangesAsync()).ReturnsAsync(expectedEstablishmentId);
            var establishmentDto = new EstablishmentDto() { Urn = new Guid().ToString(), Ukprn = null};

            //Act
            var result = await strut.CreateEstablishment(establishmentDto);

            //Assert
            Assert.Equal(expectedEstablishmentId, result);
        }
        
        
        [Fact]
        public async Task CreateEstablishmentDoesThrowsExceptionWhenUrnAndUkprnAreNotPresent()
        {
            var strut = CreateStrut();
            var establishmentDto = new EstablishmentDto() { Urn = null, Ukprn = null};
            
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await strut.CreateEstablishment(establishmentDto));

            Assert.Equal("Both Urn and Ukprn cannot be null.", exception.Message);
        }
        
    }
}