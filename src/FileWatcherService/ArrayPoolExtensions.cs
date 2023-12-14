namespace FileWatcherService;

using System.Buffers;
using System.Security.Cryptography;

public static class ArrayPoolExtensions
{
    public static ArrayPoolAllocation<T> Allocate<T>(this ArrayPool<T> pool,
                                                     int minimumLength,
                                                     bool clearBufferContents = true)
    {
        return new ArrayPoolAllocation<T>(pool, minimumLength, clearBufferContents);
    }

    public static ArrayPoolByteAllocation AllocateByte(this ArrayPool<byte> pool,
                                                       int minimumLength,
                                                       bool clearBufferContents = true)
    {
        return new ArrayPoolByteAllocation(pool, minimumLength, clearBufferContents);
    }
}

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
///     REFERENCES:
///     -   <see href="https://ericlippert.com/2011/03/14/to-box-or-not-to-box/" />
///     -   <see href="https://stackoverflow.com/questions/7914423/struct-and-idisposable" />
///     -   <see href="https://stackoverflow.com/questions/2412981/if-my-struct-implements-idisposable-will-it-be-boxed-when-used-in-a-using-statem" />
///     -   <see href="https://stackoverflow.com/questions/1330571/when-does-a-using-statement-box-its-argument-when-its-a-struct" />
///     -   <see href="" />
/// </remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not needed.")]
public readonly struct ArrayPoolAllocation<T> : IDisposable
{
    private readonly ArrayPool<T> _pool;
    private readonly bool _clearBufferContents;

    internal ArrayPoolAllocation(ArrayPool<T> pool,
                                 int minimumLength,
                                 bool clearBufferContents)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _clearBufferContents = clearBufferContents;

        Buffer = _pool.Rent(minimumLength);
    }

    public void Dispose()
    {
        if (_clearBufferContents)
        {
            // https://github.com/dotnet/runtime/discussions/48697
            Buffer.AsSpan().Clear();
        }

        _pool.Return(Buffer);
    }

    public T[] Buffer { get; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not needed.")]
public readonly struct ArrayPoolByteAllocation : IDisposable
{
    private readonly ArrayPool<byte> _pool;
    private readonly bool _clearBufferContents;

    internal ArrayPoolByteAllocation(ArrayPool<byte> pool,
                                     int minimumLength,
                                     bool clearBufferContents)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _clearBufferContents = clearBufferContents;

        Buffer = _pool.Rent(minimumLength);
    }

    public void Dispose()
    {
        if (_clearBufferContents)
        {
            // https://github.com/dotnet/runtime/discussions/48697
            CryptographicOperations.ZeroMemory(Buffer.AsSpan());
        }

        _pool.Return(Buffer);
    }

    public byte[] Buffer { get; }
}
