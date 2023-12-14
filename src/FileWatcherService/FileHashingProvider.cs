namespace FileWatcherService;

using System.Buffers;
using System.IO.Hashing;

public sealed class FileHashingProvider
{
    public string ComputeHash(string filePath, bool clearBufferContents = false)
    {
        using var sourceStream = File.OpenRead(filePath);

        using var sourceAllocation = ArrayPool<byte>.Shared.AllocateByte((int) sourceStream.Length, clearBufferContents);

        var sourceBuffer = sourceAllocation.Buffer;

        sourceStream.Read(sourceBuffer);

        using var destinationAllocation = ArrayPool<byte>.Shared.AllocateByte(byte.MaxValue, clearBufferContents);

        var destinationBuffer = (ReadOnlySpan<byte>) destinationAllocation.Buffer;

        destinationBuffer = XxHash3.Hash(sourceBuffer);
        //destinationBuffer = XxHash128.Hash(sourceBuffer);
        //destinationBuffer = XxHash64.Hash(sourceBuffer);
        //destinationBuffer = SHA256.HashData(sourceBuffer);

        return Convert.ToHexString(destinationBuffer);
    }
}
