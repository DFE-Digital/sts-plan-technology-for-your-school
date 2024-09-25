using System.Linq.Expressions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Establishments.Models;
using NSubstitute;


namespace Dfe.PlanTech.Application.UnitTests.Establishments.Queries
{
    public class GetEstablishmentIdQueryTests
    {
        private static readonly Establishment FirstEstablishment = new()
        {
            Id = 1,
            OrgName = "First establishment",
            EstablishmentRef = "Ref-One"
        };

        private static readonly Establishment SecondEstablishment = new()
        {
            Id = 2,
            OrgName = "Second establishment",
            EstablishmentRef = "Ref-Two"
        };

        private static readonly Establishment ThirdEstablishment = new()
        {
            Id = 3,
            OrgName = "Third establishment",
            EstablishmentRef = "Ref-Three"
        };

        public IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();

        public GetEstablishmentIdQuery CreateStrut()
        {
            return new GetEstablishmentIdQuery(Db);
        }

        private readonly List<Establishment> _establishments = new() { FirstEstablishment, SecondEstablishment, ThirdEstablishment };

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetEstablishmentId_Returns_TheCorrectEstablishmentId(int establishmentId)
        {
            //Arrange
            var strut = CreateStrut();

            var expectedEstablishment = _establishments.FirstOrDefault(establishment => establishment.Id == establishmentId);

            Assert.NotNull(expectedEstablishment);

            Db.GetEstablishmentBy(Arg.Any<Expression<Func<Establishment, bool>>>()).Returns(callInfo =>
            {
                var query = callInfo.ArgAt<Expression<Func<Establishment, bool>>>(0);

                return _establishments.AsQueryable().FirstOrDefault(query);
            });

            //Act
            var result = await strut.GetEstablishmentId(expectedEstablishment.EstablishmentRef);

            //Assert
            Assert.Equal(expectedEstablishment.Id, result);
        }
    }
}
