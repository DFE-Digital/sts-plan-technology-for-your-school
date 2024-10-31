using System.Security.Cryptography;

namespace Dfe.PlanTech.Domain.Helpers;

public static class RandomNumber
{
    public static int GenerateRandomInt(int min, int max)
    {
        if (min >= max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), "Minimum value must be less than maximum value.");
        }

        long range = (long)max - min;

        byte[] randomNumber = new byte[4];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        uint randomValue = BitConverter.ToUInt32(randomNumber, 0);

        return (int)(min + (randomValue % (uint)range));
    }
}
