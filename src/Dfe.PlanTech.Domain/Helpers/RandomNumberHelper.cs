using System.Security.Cryptography;

namespace Dfe.PlanTech.Domain.Helpers;

public static class RandomNumberHelper
{
    /// <summary>
    /// Generates a random integer within the specified range.
    /// </summary>
    /// <param name="min">The minimum value of the range (inclusive). Must be smaller than max.</param>
    /// <param name="max">The maximum value of the range (exclusive).</param>
    /// <returns>A random integer between min and max.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when min is greater than or equal to max.</exception>
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
