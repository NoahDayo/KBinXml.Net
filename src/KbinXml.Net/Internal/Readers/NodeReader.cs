using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace KbinXml.Net.Internal.Readers;

internal ref partial struct NodeReader : IKBinReader
{
    private const int KbinLengthMask = 0xBF;

    private readonly ReadOnlySpan<byte> _span;
    private readonly Encoding _encoding;
    private readonly bool _compressed;

    private int _position;
    private ulong _lastCompressedKey;
    private byte _lastCompressedLength;
    private string? _lastCompressedValue;

    public NodeReader(ReadOnlySpan<byte> span, Encoding encoding, bool compressed)
    {
        _span = span;
        _compressed = compressed;
        _encoding = encoding;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueReadResult<string> ReadString()
    {
        var result = ReadU8();
        var length = result.Value;
        return _compressed
            ? ReadCompressedString(length)
            : ReadUncompressedString(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueReadResult<string> ReadCompressedString(byte length)
    {
        var byteCount = (length * 6 + 7) / 8;
        var spanResult = ReadBytes(byteCount);
        // Short names fit in a lossless key; leave longer names on the direct decode path.
        var readString = byteCount <= 5 && spanResult.Span.Length == byteCount
            ? DecodeShortCached(spanResult.Span, length)
            : SixbitHelper.Decode(spanResult.Span, length);
        return new ValueReadResult<string>
        (
            readString
#if USELOG
            , spanResult.ReadStatus
#endif
        );
    }

    // Keep the lookup code out of the common long-name path.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private string DecodeShortCached(ReadOnlySpan<byte> bytes, byte length)
    {
        ulong key = 0;
        for (var i = 0; i < bytes.Length; i++)
            key |= (ulong)bytes[i] << (i * 8);

        if (_lastCompressedValue != null && _lastCompressedLength == length && _lastCompressedKey == key)
            return _lastCompressedValue;

        var value = SixbitHelper.Decode(bytes, length);
        _lastCompressedKey = key;
        _lastCompressedLength = length;
        _lastCompressedValue = value;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe ValueReadResult<string> ReadUncompressedString(byte length)
    {
        var readSpanResult = ReadBytes((length & KbinLengthMask) + 1);

#if NET8_0_OR_GREATER
        return new ValueReadResult<string>
        (
            _encoding.GetString(readSpanResult.Span)
#if USELOG
            , readSpanResult.ReadStatus
#endif
        );
#else
        fixed (byte* p = readSpanResult.Span)
        {
            return new ValueReadResult<string>
            (
                _encoding.GetString(p, readSpanResult.Span.Length)
#if USELOG
                , readSpanResult.ReadStatus
#endif
            );
        }
#endif
    }
}
