using System.Text;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class GZipRedisValueCompressorTests
{
    [Fact]
    public void Compress_ShouldReturnFalse_WhenValueIsNull()
    {
        byte[] value = null!;
        var result = GZipRedisValueCompressor.Compress(ref value!);
        Assert.False(result);
    }

    [Fact]
    public void Compress_ShouldReturnFalse_WhenValueIsBelowThreshold()
    {
        byte[] value = new byte[199];
        var result = GZipRedisValueCompressor.Compress(ref value);
        Assert.False(result);
        Assert.Equal(199, value.Length);
    }

    [Fact]
    public void Compress_ShouldReturnFalse_WhenValueAlreadyCompressed()
    {
        byte[] value = new byte[200];
        Array.Copy(new byte[] { 0x1f, 0x8b }, value, 2);
        var result = GZipRedisValueCompressor.Compress(ref value);
        Assert.False(result);
        Assert.Equal(200, value.Length);
    }

    [Fact]
    public void Decompress_ShouldReturnFalse_WhenValueIsNull()
    {
        byte[] value = null!;
        var result = GZipRedisValueCompressor.Decompress(ref value);
        Assert.False(result);
    }

    [Fact]
    public void Decompress_ShouldReturnFalse_WhenValueIsEmpty()
    {
        byte[] value = [];
        var result = GZipRedisValueCompressor.Decompress(ref value);
        Assert.False(result);
    }

    [Fact]
    public void Compress_ShouldWorkWithRedisValue()
    {
        var input = new string('A', 1000);
        var redisValue = new RedisValue(input);
        var compressedValue = GZipRedisValueCompressor.Compress(redisValue);
        Assert.True(compressedValue.HasValue);
        Assert.True(compressedValue.Length() < 1000);
    }

    [Fact]
    public void Decompress_ShouldWorkWithRedisValue()
    {
        var originalValue = new string('A', 1000);
        var redisValue = new RedisValue(originalValue);
        GZipRedisValueCompressor.Compress(redisValue);

        var decompressedValue = GZipRedisValueCompressor.Decompress(redisValue);
        Assert.Equal(originalValue.Length, decompressedValue.Length());
    }

    [Fact]
    public void Decompress_ValidGZipInput_ReturnsTrue()
    {
        var input = new string('A', 1000);
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] compressed = [.. inputBytes];

        GZipRedisValueCompressor.Compress(ref compressed);


        byte[] decompressed = [.. compressed];
        GZipRedisValueCompressor.Decompress(ref decompressed);

        var decompressedString = Encoding.UTF8.GetString(decompressed);

        Assert.Equal(input, decompressedString);
    }
}
