using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Moq;

using System.Linq.Expressions;


namespace Dfe.PlanTech.Application.UnitTests.Establishment.Queries
{
    public class GetEstablishmentIdQueryTests
    {
        public Mock<IPlanTechDbContext> mockDb = new Mock<IPlanTechDbContext>();

        public GetEstablishmentIdQuery CreateStrut()
        {
            return new GetEstablishmentIdQuery(mockDb.Object);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetEstablishmentId_Returns_TheCorrectEstablishmentId(int establishmentId)
        {
            //Arrange
            var strut = CreateStrut();
            var establishmentRef = new Guid().ToString();
            var returnedEstablishment = new Domain.Establishments.Models.Establishment() { EstablishmentRef = establishmentRef, Id = establishmentId };
            mockDb.Setup(x => x.GetEstablishmentBy(It.IsAny<Expression<Func<Domain.Establishments.Models.Establishment, bool>>>())).ReturnsAsync(returnedEstablishment);

            //Act
            var result = await strut.GetEstablishmentId(establishmentRef);

            //Assert
            Assert.Equal(establishmentId, result);
        }
    }
}