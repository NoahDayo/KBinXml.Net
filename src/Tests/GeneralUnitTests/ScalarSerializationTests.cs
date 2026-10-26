using System;
using KbinXml.Net.Internal;
using KbinXml.Net.Utils;
using Xunit;

namespace GeneralUnitTests;

public class ScalarSerializationTests
{
    [Fact]
    public void DirectScalarSerializationMatchesBuilderPath()
    {
        Span<char> chars = stackalloc char[257];
        for (byte id = 2; id <= 15; id++)
        {
            if (id is 10 or 11) continue; // Binary and string nodes have separate readers.

            var type = NodeTypeFactory.GetNodeType(id);
            for (var pattern = 0; pattern < 4; pattern++)
            {
                var bytes = new byte[type.Size];
                for (var i = 0; i < bytes.Length; i++)
                    bytes[i] = pattern switch
                    {
                        0 => (byte)0,
                        1 => (byte)0xFF,
                        2 => (byte)i,
                        _ => (byte)(i * 37 + 11)
                    };

                var builder = new ValueStringBuilder(chars);
                type.SerializeAppend(ref builder, bytes);
                Assert.Equal(builder.ToString(), type.Serialize(bytes));
            }
        }
    }
}
