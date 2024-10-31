using System.Security.Cryptography;

namespace Dfe.PlanTech.Domain.Helpers;

public static class RandomNumber
{
    [ThreadStatic]
    private static RandomNumberGenerator? _local;

    public static RandomNumberGenerator Local
    {
        get
        {
            _local ??= RandomNumberGenerator.Create();
            return _local;
        }
    }
}
