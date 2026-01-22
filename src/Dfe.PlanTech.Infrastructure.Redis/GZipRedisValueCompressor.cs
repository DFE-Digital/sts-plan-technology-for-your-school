using System.IO.Compression;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

/// <summary>
/// Provides compression + decompression functionality for Redis values using GZip
/// </summary>
public static class GZipRedisValueCompressor
{
    private const int CompressionThreshold = 200;
    private static readonly byte[] _gzipHeader = [0x1f, 0x8b];

    /// <summary>
    /// Compress the provided Redis value using GZip.
    /// </summary>
    /// <param name="value">The object to compress</param>
    /// <returns>True if it was compressed, false if it was not compressed</returns>
    public static bool Compress(ref byte[] value)
    {
        if (
            value == null
            || value.Length < CompressionThreshold
            || value.Take(_gzipHeader.Length).SequenceEqual(_gzipHeader)
        )
        {
            return false;
        }

        using var outputStream = new MemoryStream();

        using var inputStream = new MemoryStream(value);
        using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress, true);
        inputStream.CopyTo(gzipStream);
        gzipStream.Close();

        value = outputStream.ToArray();

        return true;
    }

    /// <summary>
    /// Compresses the provided Redis value using GZip
    /// </summary>
    /// <param name="redisValue"></param>
    /// <returns></returns>
    public static RedisValue Compress(RedisValue redisValue)
    {
        if (redisValue.IsNull || !redisValue.HasValue)
        {
            return redisValue;
        }

        byte[] valueBlob = redisValue!;
        Compress(ref valueBlob);
        return valueBlob;
    }

    /// <summary>
    /// Decompresses the provided value using GZip
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if decompressed, false if not.</returns>
    public static bool Decompress(ref byte[] value)
    {
        if (
            value == null
            || value.Length == 0
            || !value.Take(_gzipHeader.Length).SequenceEqual(_gzipHeader)
        )
        {
            return false;
        }

        using var inputStream = new MemoryStream(value);
        using var outputStream = new MemoryStream();
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress, true);

        gzipStream.CopyTo(outputStream);

        value = outputStream.ToArray();
        return true;
    }

    /// <summary>
    /// Decompresses the provided Redis value using GZip
    /// </summary>
    /// <param name="redisValue"></param>
    /// <returns></returns>
    public static RedisValue Decompress(RedisValue redisValue)
    {
        if (redisValue.IsNull || !redisValue.HasValue)
        {
            return redisValue;
        }

        byte[] valueBlob = redisValue!;
        Decompress(ref valueBlob);
        return valueBlob;
    }
}
