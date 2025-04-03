using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Domain.UnitTests.Establishments.Models
{
    public class EstablishmentLinkTests
    {
        [Fact]
        public void EstablishmentLink_Properties_Should_Be_Set_And_Retrieved_Correctly()
        {
            var establishmentLink = new EstablishmentLink
            {
                Id = 1,
                GroupUid = "1234",
                EstablishmentName = "Test Establishment",
                Urn = "12345"
            };

            Assert.Equal(1, establishmentLink.Id);
            Assert.Equal("1234", establishmentLink.GroupUid);
            Assert.Equal("Test Establishment", establishmentLink.EstablishmentName);
            Assert.Equal("12345", establishmentLink.Urn);
        }
    }
}
