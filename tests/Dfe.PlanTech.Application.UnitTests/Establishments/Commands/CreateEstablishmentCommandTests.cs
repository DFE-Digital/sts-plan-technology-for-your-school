using Dfe.PlanTech.Application.Users.Commands;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Establishments.Commands
{
    public class CreateEstablishmentCommandTests
    {
        private const string OrgName = "Org name";
        private readonly EstablishmentTypeDto EstablishmentType = new() { Name = "Establishment Type" };

        public IPlanTechDbContext Db = Substitute.For<IPlanTechDbContext>();

        public CreateEstablishmentCommand CreateStrut()
        {
            return new CreateEstablishmentCommand(Db);
        }

        [Fact]
        public async Task CreateEstablishmentReturnsIdOfNewlyCreateEstablishmentWithUkprn()
        {
            var establishmentId = 1;
            var establishment = new Establishment();

            //Arrange
            var strut = CreateStrut();
            Db.When(db => db.AddEstablishment(Arg.Any<Establishment>()))
            .Do(callinfo =>
            {
                var dto = callinfo.ArgAt<Establishment>(0);

                establishment = dto;
                establishment.Id = establishmentId;
            });

            Db.SaveChangesAsync().Returns(Task.FromResult(establishment.Id));

            var establishmentDto = new EstablishmentDto() { Urn = null, Ukprn = new Guid().ToString(), Type = EstablishmentType, OrgName = OrgName };

            //Act
            var result = await strut.CreateEstablishment(establishmentDto);

            //Assert
            Assert.Equal(establishmentId, result);
        }

        [Fact]
        public async Task CreateEstablishmentReturnsIdOfNewlyCreateEstablishmentWithUrn()
        {
            //Arrange
            var establishmentId = 1;
            var establishment = new Establishment();

            //Arrange
            var strut = CreateStrut();
            Db.When(db => db.AddEstablishment(Arg.Any<Establishment>()))
            .Do(callinfo =>
            {
                var dto = callinfo.ArgAt<Establishment>(0);

                establishment = dto;
                establishment.Id = establishmentId;
            });

            Db.SaveChangesAsync().Returns(Task.FromResult(establishment.Id));
            var establishmentDto = new EstablishmentDto() { Urn = new Guid().ToString(), Ukprn = null, Type = EstablishmentType, OrgName = OrgName };

            //Act
            var result = await strut.CreateEstablishment(establishmentDto);

            //Assert
            Assert.Equal(establishmentId, result);
        }


        [Fact]
        public async Task CreateEstablishmentDoesThrowsExceptionWhenUrnAndUkprnAreNotPresent()
        {
            var strut = CreateStrut();
            var establishmentDto = new EstablishmentDto() { Urn = null, Ukprn = null, OrgName = OrgName, Type = EstablishmentType };

            var exception = await Assert.ThrowsAsync<InvalidEstablishmentException>(() => strut.CreateEstablishment(establishmentDto));

            Assert.Equal(EstablishmentDto.InvalidEstablishmentErrorMessage, exception.Message);
        }

    }
}
