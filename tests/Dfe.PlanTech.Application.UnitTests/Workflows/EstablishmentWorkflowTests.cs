using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class EstablishmentWorkflowTests
{
    private readonly IEstablishmentRepository _estRepo = Substitute.For<IEstablishmentRepository>();
    private readonly IEstablishmentLinkRepository _linkRepo = Substitute.For<IEstablishmentLinkRepository>();
    private readonly IStoredProcedureRepository _spRepo = Substitute.For<IStoredProcedureRepository>();

    private EstablishmentWorkflow CreateServiceUnderTest() =>
        new EstablishmentWorkflow(_estRepo, _linkRepo, _spRepo);

    // ── Helpers: create minimal entities that map via AsDto() in YOUR codebase ──
    // Replace these types with your real entity classes (what the repos return),
    // and set properties so that AsDto() maps to the asserted DTOs.
    private EstablishmentEntity MakeEstablishment(int id, string urn, string name)
        => new EstablishmentEntity { Id = id, EstablishmentRef = urn, OrgName = name };

    private EstablishmentLinkEntity MakeLink(int id, string urn, string name)
        => new EstablishmentLinkEntity { Id = id, Urn = urn, EstablishmentName = name };

    private GroupReadActivityEntity MakeRead(int id, int userId, int selectedEstablishmentId)
        => new GroupReadActivityEntity { Id = id, UserId = userId, SelectedEstablishmentId = selectedEstablishmentId };

    // ────────────────────────────────────────────────────────────────────────────
    // GetOrCreateEstablishmentAsync(EstablishmentModel)
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateEstablishment_Returns_Existing_When_Found()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        var urn = "12345";
        var name = "testName";
        var model = new EstablishmentModel { Id = Guid.NewGuid(), Urn = urn, Name = name };
        var establishment = MakeEstablishment(10, urn, name);

        _estRepo.GetEstablishmentByReferenceAsync(urn).Returns(establishment);

        var dto = await serviceUnderTest.GetOrCreateEstablishmentAsync(model);

        // Assert DTO reflects the existing entity
        Assert.Equal(10, dto.Id);
        Assert.Equal(urn, dto.EstablishmentRef ?? dto.EstablishmentRef);
        Assert.Equal(name, dto.OrgName);

        await _estRepo.Received(1).GetEstablishmentByReferenceAsync(urn);
        await _estRepo.DidNotReceive().CreateEstablishmentFromModelAsync(Arg.Any<EstablishmentModel>());
    }

    [Fact]
    public async Task GetOrCreateEstablishment_Creates_When_Not_Found()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        var model = new EstablishmentModel { Urn = "999", Name = "New School" };

        _estRepo.GetEstablishmentByReferenceAsync("999")
                .Returns((EstablishmentEntity?)null);

        _estRepo.CreateEstablishmentFromModelAsync(model)
                .Returns(MakeEstablishment(77, "999", "New School"));

        var dto = await serviceUnderTest.GetOrCreateEstablishmentAsync(model);

        Assert.Equal(77, dto.Id);
        Assert.Equal("999", dto.EstablishmentRef ?? dto.EstablishmentRef);
        Assert.Equal("New School", dto.OrgName);

        await _estRepo.Received(1).CreateEstablishmentFromModelAsync(model);
    }

    // Overload: GetOrCreateEstablishmentAsync(string urn, string name)
    [Fact]
    public async Task GetOrCreateEstablishment_Overload_Builds_Model_And_Delegates()
    {
        var serviceUnderTest = CreateServiceUnderTest();

        var establishmentReference = "ABC123";
        var name = "testName";

        _estRepo.GetEstablishmentByReferenceAsync(establishmentReference)
                .Returns((EstablishmentEntity?)null);

        _estRepo.CreateEstablishmentFromModelAsync(
                Arg.Is<EstablishmentModel>(m => m.Urn == establishmentReference && m.Name == name))
            .Returns(MakeEstablishment(5, establishmentReference, name));

        var dto = await serviceUnderTest.GetOrCreateEstablishmentAsync(establishmentReference, name);

        Assert.Equal(5, dto.Id);
        Assert.Equal(name, dto.OrgName);

        await _estRepo.Received(1).GetEstablishmentByReferenceAsync(establishmentReference);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // GetEstablishmentByReferenceAsync
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEstablishmentByReference_Returns_First_Or_Null()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        var establishments = new[] { MakeEstablishment(1, "U1", "One") }.ToList();

        _estRepo.GetEstablishmentsByReferencesAsync(Arg.Is<IEnumerable<string>>(r => r.Single() == "U1"))
                .Returns(establishments);

        var found = await serviceUnderTest.GetEstablishmentByReferenceAsync("U1");
        Assert.NotNull(found);
        Assert.Equal(1, found!.Id);

        _estRepo.GetEstablishmentsByReferencesAsync(Arg.Is<IEnumerable<string>>(r => r.Single() == "MISSING"))
                .Returns([]);

        var missing = await serviceUnderTest.GetEstablishmentByReferenceAsync("MISSING");
        Assert.Null(missing);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // GetEstablishmentsByReferencesAsync
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEstablishmentsByReferences_Maps_All()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        var establishments = new[]
        {
            MakeEstablishment(11, "A", "A School"),
            MakeEstablishment(22, "B", "B School")
        }.ToList();

        _estRepo.GetEstablishmentsByReferencesAsync(Arg.Is<IEnumerable<string>>(r => r.SequenceEqual(new[] { "A", "B" })))
                .Returns(establishments);

        var list = (await serviceUnderTest.GetEstablishmentsByReferencesAsync(new[] { "A", "B" })).ToList();

        Assert.Collection(list,
            e => { Assert.Equal(11, e.Id); Assert.Equal("A School", e.OrgName); },
            e => { Assert.Equal(22, e.Id); Assert.Equal("B School", e.OrgName); });
    }

    // ────────────────────────────────────────────────────────────────────────────
    // GetGroupEstablishments
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetGroupEstablishments_Maps_Links()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        var establishments = new[]
        {
            MakeLink(100, "URN-1", "Child 1"),
            MakeLink(200, "URN-2", "Child 2")
        }.ToList();

        _linkRepo.GetGroupEstablishmentsByEstablishmentIdAsync(42)
                 .Returns(establishments);

        var links = await serviceUnderTest.GetGroupEstablishments(42);

        Assert.Collection(links,
            l => { Assert.Equal(100, l.Id); Assert.Equal("URN-1", l.Urn); Assert.Equal("Child 1", l.EstablishmentName); },
            l => { Assert.Equal(200, l.Id); Assert.Equal("URN-2", l.Urn); Assert.Equal("Child 2", l.EstablishmentName); });
    }

    // ────────────────────────────────────────────────────────────────────────────
    // RecordGroupSelection
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RecordGroupSelection_Delegates_To_StoredProcedure()
    {
        var serviceUnderTest = CreateServiceUnderTest();
        var model = new UserGroupSelectionModel
        {
            UserId = 7,
            UserEstablishmentId = 1,
            SelectedEstablishmentId = 2,
            SelectedEstablishmentName = "Pick"
        };

        _spRepo.RecordGroupSelection(model).Returns(999);

        var id = await serviceUnderTest.RecordGroupSelection(model);

        Assert.Equal(999, id);
        await _spRepo.Received(1).RecordGroupSelection(model);
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Constructor guard clauses
    // ────────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Ctor_Null_Repos_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new EstablishmentWorkflow(null!, _linkRepo, _spRepo));
        Assert.Throws<ArgumentNullException>(() => new EstablishmentWorkflow(_estRepo, null!, _spRepo));
        Assert.Throws<ArgumentNullException>(() => new EstablishmentWorkflow(_estRepo, _linkRepo, null!));
    }
}
