using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options) : base(options)
    {
    }

    protected DataProtectionDbContext()
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}
