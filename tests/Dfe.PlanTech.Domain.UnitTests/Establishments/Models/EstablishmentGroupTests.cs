using Dfe.PlanTech.Domain.Establishments.Models;

namespace Dfe.PlanTech.Domain.UnitTests.Establishments.Models
{
    public class EstablishmentGroupTests
    {
        [Fact]
        public void EstablishmentGroup_Properties_Should_Be_Set_And_Retrieved_Correctly()
        {
            var establishmentGroup = new EstablishmentGroup
            {
                Id = 1,
                Uid = "1234",
                GroupName = "MAT Test Group",
                GroupType = "Multi-academy trust",
                GroupStatus = "Open"
            };

            Assert.Equal(1, establishmentGroup.Id);
            Assert.Equal("1234", establishmentGroup.Uid);
            Assert.Equal("MAT Test Group", establishmentGroup.GroupName);
            Assert.Equal("Multi-academy trust", establishmentGroup.GroupType);
            Assert.Equal("Open", establishmentGroup.GroupStatus);
        }
    }
}
