using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Establishments.Models;
using NSubstitute;
using System.Linq.Expressions;


namespace Dfe.PlanTech.Application.UnitTests.Establishments.Queries
{
    public class GetEstablishmentIdQueryTests
    {
        public IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();

        public GetEstablishmentIdQuery CreateStrut()
        {
            return new GetEstablishmentIdQuery(Db);
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
            var returnedEstablishment = new Establishment() { EstablishmentRef = establishmentRef, Id = establishmentId };
            Db.GetEstablishmentBy(Arg.Any<Expression<Func<Establishment, bool>>>()).Returns(returnedEstablishment);

            //Act
            var result = await strut.GetEstablishmentId(establishmentRef);

            //Assert
            Assert.Equal(establishmentId, result);
        }
    }
}