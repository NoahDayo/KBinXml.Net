using System.Linq;
using KbinXml.Net;
using Xunit;

namespace GeneralUnitTests;

public class CompressedNameTests
{
    [Fact]
    public void RepeatedShortNamesRemainDistinct()
    {
        const string xml = "<root><a/><a/><ab/><ab/><a/><abcdef/><abcdefg/></root>";
        var binary = KbinConverter.Write(xml, KnownEncodings.UTF8,
            new WriteOptions { Compress = true });

        var document = KbinConverter.ReadXmlLinq(binary);
        Assert.Equal(new[] { "a", "a", "ab", "ab", "a", "abcdef", "abcdefg" },
            document.Root!.Elements().Select(element => element.Name.LocalName));
    }
}
