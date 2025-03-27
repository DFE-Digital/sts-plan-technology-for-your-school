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

        private static readonly EstablishmentLink FirstEstablishmentLink = new()
        {
            Id = 1,
            GroupUid = "GroupUID123",
            EstablishmentName = "First Establishment",
            Urn = 12345
        };

        private static readonly EstablishmentLink SecondEstablishmentLink = new()
        {
            Id = 2,
            GroupUid = "GroupUID456",
            EstablishmentName = "Second Establishment",
            Urn = 67890
        };

        public IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();

        public GetEstablishmentIdQuery CreateStrut()
        {
            return new GetEstablishmentIdQuery(Db);
        }

        private readonly List<Establishment> _establishments = new() { FirstEstablishment, SecondEstablishment, ThirdEstablishment };
        private readonly List<EstablishmentLink> _establishmentLinks = new() { FirstEstablishmentLink, SecondEstablishmentLink };

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

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetGroupEstablishmentsById_Returns_Correct_Establishment_Links(int establishmentId)
        {
            // Arrange
            var query = new GetEstablishmentIdQuery(Db);

            Db.GetGroupEstablishmentsBy(Arg.Any<Expression<Func<Establishment, bool>>>())
                     .Returns(_establishmentLinks);

            // Act
            var result = await query.GetGroupEstablishments(establishmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_establishmentLinks.Count, result.Count);
            Assert.Contains(_establishmentLinks, link => link.Id == establishmentId);
            ;
        }
    }
}
