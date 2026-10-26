using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;

namespace GeneralUnitTests;

public class BinaryReaderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BinaryValueIsWrittenAfterAttributesWithRequestedCasing(bool upper)
    {
        // Cross the writer's hex chunk boundary with every possible hex digit.
        var hex = string.Concat(Enumerable.Repeat("0123456789abcdef", 600));
        var xml = $"<root><data __type=\"bin\" __size=\"{hex.Length / 2}\" tag=\"kept\">{hex}</data>" +
                  "<empty __type=\"bin\" __size=\"0\"></empty><value __type=\"s32\">42</value></root>";
        var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
        var options = new ReadOptions { BinaryUppercase = upper };

        var output = KbinConverter.ReadXmlBytes(kbin, options);
        using var stream = KbinConverter.GetXmlStream(kbin, options);
        Assert.Equal(output, stream.ToArray());

        using var outputStream = new MemoryStream(output);
        var document = XDocument.Load(outputStream);
        var data = document.Root!.Element("data")!;
        Assert.Equal(upper ? hex.ToUpperInvariant() : hex, data.Value);
        Assert.Equal("bin", data.Attribute("__type")!.Value);
        Assert.Equal((hex.Length / 2).ToString(), data.Attribute("__size")!.Value);
        Assert.Equal("kept", data.Attribute("tag")!.Value);
        Assert.Equal(string.Empty, document.Root.Element("empty")!.Value);
        Assert.Equal("42", document.Root.Element("value")!.Value);

        var linq = KbinConverter.ReadXmlLinq(kbin, options);
        Assert.Equal(data.Value, linq.Root!.Element("data")!.Value);
    }
}
