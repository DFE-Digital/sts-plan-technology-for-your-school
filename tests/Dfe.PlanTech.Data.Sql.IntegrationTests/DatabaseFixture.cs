using DbUp;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests;

public class DatabaseFixture : IAsyncLifetime
{
    // TEST CONTAINER ONLY: This password is only for the ephemeral test SQL Server
    // spun up via test containers for integration tests. It is NOT used in production,
    // development, or any persistent environment.
    // This password is safe to commit and is not a security risk.
    private const string TestContainerPassword = "yourStrong(!)Password";

    public MsSqlContainer DbContainer { get; private set; } = null!;
    public string ConnectionString => DbContainer.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        DbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(TestContainerPassword)
            .Build();
        await DbContainer.StartAsync();

        var filterOutput = new List<string>();

        // Run DbUp migrations
        EnsureDatabase.For.SqlDatabase(ConnectionString);
        var upgrader = DeployChanges
            .To.SqlDatabase(ConnectionString)
            .WithScriptsEmbeddedInAssembly(
                typeof(DatabaseUpgrader.Options).Assembly,
                s =>
                {
                    filterOutput.Add(s);
                    return s.StartsWith(
                        "Dfe.PlanTech.DatabaseUpgrader.Scripts",
                        StringComparison.Ordinal
                    );
                }
            )
            .LogToConsole()
            .Build();
        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            throw new Exception($"DbUp migration failed: {result.Error}");
        }

        await SeedGiasReferenceDataAsync(
            CreateDbContext(),
            TestContext.Current.CancellationToken
        );
    }

    /// <summary>
    /// Creates a new DbContext with transaction isolation for tests.
    /// Each test should call this to get a fresh, isolated context.
    /// </summary>
    public PlanTechDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PlanTechDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new PlanTechDbContext(options);
    }

    public async ValueTask DisposeAsync()
    {
        await DbContainer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private static async Task SeedGiasReferenceDataAsync(
        DbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[groupStatus]
                    WHERE [groupStatusCode] = 'TEST'
                )
                BEGIN
                    INSERT INTO [gias].[groupStatus]
                        ([groupStatusCode], [groupStatusName])
                    VALUES
                        ('TEST', 'Test Group Status');
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[groupType]
                    WHERE [groupTypeCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[groupType]
                        ([groupTypeCode], [groupTypeName])
                    VALUES
                        (999, 'Test Group Type');
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[establishmentStatus]
                    WHERE [establishmentStatusCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[establishmentStatus]
                        ([establishmentStatusCode], [establishmentStatusName])
                    VALUES
                        (999, 'Test Establishment Status');
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[gender]
                    WHERE [genderCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[gender]
                        ([genderCode], [genderName])
                    VALUES
                        (999, 'Test Gender');
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[localAuthority]
                    WHERE [localAuthorityCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[localAuthority]
                        ([localAuthorityCode], [localAuthorityName])
                    VALUES
                        (999, 'Test Local Authority');
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[phase]
                    WHERE [phaseCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[phase]
                        ([phaseCode], [phaseName])
                    VALUES
                        (999, 'Test Phase');
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[typeOfEstablishment]
                    WHERE [typeOfEstablishmentCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[typeOfEstablishment]
                        ([typeOfEstablishmentCode], [typeOfEstablishmentName])
                    VALUES
                        (999, 'Test Establishment Type');
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[administrativeDistrict]
                    WHERE [administrativeDistrictCode] = 'TESTDIST'
                )
                BEGIN
                    INSERT INTO [gias].[administrativeDistrict]
                        (
                            [administrativeDistrictCode],
                            [administrativeDistrictName]
                        )
                    VALUES
                        (
                            'TESTDIST',
                            'Test Administrative District'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[administrativeWard]
                    WHERE [administrativeWardCode] = 'TESTWARD'
                )
                BEGIN
                    INSERT INTO [gias].[administrativeWard]
                        (
                            [administrativeWardCode],
                            [administrativeWardName]
                        )
                    VALUES
                        (
                            'TESTWARD',
                            'Test Administrative Ward'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[admissionsPolicy]
                    WHERE [admissionsPolicyCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[admissionsPolicy]
                        (
                            [admissionsPolicyCode],
                            [admissionsPolicyName]
                        )
                    VALUES
                        (
                            999,
                            'Test Admissions Policy'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[establishmentTypeGroup]
                    WHERE [establishmentTypeGroupCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[establishmentTypeGroup]
                        (
                            [establishmentTypeGroupCode],
                            [establishmentTypeGroupName]
                        )
                    VALUES
                        (
                            999,
                            'Test Establishment Type Group'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[governmentOfficeRegion]
                    WHERE [governmentOfficeRegionCode] = 'TEST'
                )
                BEGIN
                    INSERT INTO [gias].[governmentOfficeRegion]
                        (
                            [governmentOfficeRegionCode],
                            [governmentOfficeRegionName]
                        )
                    VALUES
                        (
                            'TEST',
                            'Test Government Office Region'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[parliamentaryConstituency]
                    WHERE [parliamentaryConstituencyCode] = 'TEST'
                )
                BEGIN
                    INSERT INTO [gias].[parliamentaryConstituency]
                        (
                            [parliamentaryConstituencyCode],
                            [parliamentaryConstituencyName]
                        )
                    VALUES
                        (
                            'TEST',
                            'Test Parliamentary Constituency'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[religiousCharacter]
                    WHERE [religiousCharacterCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[religiousCharacter]
                        (
                            [religiousCharacterCode],
                            [religiousCharacterName]
                        )
                    VALUES
                        (
                            999,
                            'Test Religious Character'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[sixthFormStatus]
                    WHERE [sixthFormStatusCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[sixthFormStatus]
                        (
                            [sixthFormStatusCode],
                            [sixthFormStatusName]
                        )
                    VALUES
                        (
                            999,
                            'Test Sixth Form Status'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[trustSchoolFlag]
                    WHERE [trustSchoolFlagCode] = 999
                )
                BEGIN
                    INSERT INTO [gias].[trustSchoolFlag]
                        (
                            [trustSchoolFlagCode],
                            [trustSchoolFlagName]
                        )
                    VALUES
                        (
                            999,
                            'Test Trust School Flag'
                        );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [gias].[urbanRuralClassification]
                    WHERE [urbanRuralCode] = 'TEST'
                )
                BEGIN
                    INSERT INTO [gias].[urbanRuralClassification]
                        (
                            [urbanRuralCode],
                            [urbanRuralName]
                        )
                    VALUES
                        (
                            'TEST',
                            'Test Urban Rural Classification'
                        );
                END;
                """,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to seed GIAS reference data", ex);
        }
    }
}
