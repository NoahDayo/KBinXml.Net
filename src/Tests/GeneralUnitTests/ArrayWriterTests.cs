using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;

namespace GeneralUnitTests;

public class ArrayWriterTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ByteArrayOutputMatchesStreamOutput(bool compress)
    {
        var hex = new string('a', 8192);
        var xml = $"<root><first __type=\"s32\" tag=\"x\">42</first>" +
                  $"<data __type=\"bin\">{hex}</data><last __type=\"str\">done</last></root>";
        var bytes = Encoding.UTF8.GetBytes(xml);
        var options = new WriteOptions { Compress = compress };

        var arrayResult = KbinConverter.Write(bytes, KnownEncodings.UTF8, options);
        using var stream = new MemoryStream();
        var count = KbinConverter.Write(bytes, stream, KnownEncodings.UTF8, options);

        Assert.Equal(count, arrayResult.Length);
        Assert.Equal(stream.ToArray(), arrayResult);

        using var textStream = new MemoryStream();
        KbinConverter.Write(xml, textStream, KnownEncodings.UTF8, options);
        Assert.Equal(textStream.ToArray(), KbinConverter.Write(xml, KnownEncodings.UTF8, options));

        var linq = XDocument.Parse(xml);
        using var linqStream = new MemoryStream();
        KbinConverter.Write(linq, linqStream, KnownEncodings.UTF8, options);
        Assert.Equal(linqStream.ToArray(), KbinConverter.Write(linq, KnownEncodings.UTF8, options));

        var document = new XmlDocument();
        document.LoadXml(xml);
        using var documentStream = new MemoryStream();
        KbinConverter.Write(document, documentStream, KnownEncodings.UTF8, options);
        Assert.Equal(documentStream.ToArray(), KbinConverter.Write(document, KnownEncodings.UTF8, options));
    }
}
