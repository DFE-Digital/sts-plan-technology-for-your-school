using Dfe.PlanTech.Domain.Establishments.Models;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Models;

public class EstablishmentDtoTests
{
    [Fact]
    public void Should_Return_Valid_If_Ukprn_NotNull_And_NotEmpty()
    {
        var establishment = new EstablishmentDto()
        {
            Ukprn = "UkPrn",
            Urn = null,
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.True(establishment.IsValid);
    }

    [Fact]
    public void Should_Return_Ukprn_When_NotNull_And_NotEmpty()
    {
        var ukprn = "UkPrn";
        var establishment = new EstablishmentDto()
        {
            Ukprn = ukprn,
            Urn = null,
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.Equal(ukprn, establishment.Reference);
    }

    [Fact]
    public void Should_Return_Valid_If_Urn_NotNull_And_NotEmpty()
    {
        var establishment = new EstablishmentDto()
        {
            Ukprn = null,
            Urn = "Urn",
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.True(establishment.IsValid);
    }

    [Fact]
    public void Should_Return_Urn_When_NotNull_And_NotEmpty()
    {
        var urn = "Urn";
        var establishment = new EstablishmentDto()
        {
            Ukprn = null,
            Urn = urn,
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.Equal(urn, establishment.Reference);
    }

    [Fact]
    public void Should_Return_Invalid_When_Ids_Null()
    {
        var establishment = new EstablishmentDto()
        {
            Ukprn = null,
            Urn = null,
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.False(establishment.IsValid);
    }

    [Fact]
    public void Should_Return_Invalid_When_Ids_Empty()
    {
        var establishment = new EstablishmentDto()
        {
            Ukprn = "",
            Urn = "",
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.False(establishment.IsValid);
    }

    [Fact]
    public void Should_Throw_Exception_When_Ids_Null()
    {
        var establishment = new EstablishmentDto()
        {
            Ukprn = null,
            Urn = null,
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.ThrowsAny<Exception>(() =>
        {
            var refrence = establishment.Reference;
        });
    }

    [Fact]
    public void Should_Throw_Exception_When_Ids_Empty()
    {
        var establishment = new EstablishmentDto()
        {
            Ukprn = "",
            Urn = "",
            OrgName = "Establishment",
            Type = new EstablishmentTypeDto() { Name = "Establishment Type" },
        };

        Assert.ThrowsAny<Exception>(() =>
        {
            var refrence = establishment.Reference;
        });
    }
}
