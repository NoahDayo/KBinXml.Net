using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using KbinXml.Net;

namespace ReadBenchmark;

[MemoryDiagnoser]
#if NET8_0
[SimpleJob(RuntimeMoniker.Net80)]
#elif NET9_0
[SimpleJob(RuntimeMoniker.Net90)]
#else
[SimpleJob(RuntimeMoniker.Net10_0)]
#endif
public class ReadThroughput
{
    private byte[] _mixed = null!;
    private byte[] _binary = null!;
    private byte[] _scalars = null!;

    [GlobalSetup]
    public void Setup()
    {
        _mixed = KbinConverter.Write(File.ReadAllText("data/small.xml"), KnownEncodings.UTF8);

        var binaryXml = new StringBuilder("<root>");
        var hex = new string('a', 8192);
        for (var i = 0; i < 16; i++)
            binaryXml.Append("<data __type=\"bin\">").Append(hex).Append("</data>");
        binaryXml.Append("</root>");
        _binary = KbinConverter.Write(binaryXml.ToString(), KnownEncodings.UTF8);

        var scalarXml = new StringBuilder("<root>");
        for (var i = 0; i < 1000; i++)
            scalarXml.Append("<value __type=\"s32\">12345</value>");
        scalarXml.Append("</root>");
        _scalars = KbinConverter.Write(scalarXml.ToString(), KnownEncodings.UTF8);
    }

    [Benchmark]
    public byte[] Mixed() => KbinConverter.ReadXmlBytes(_mixed);

    [Benchmark]
    public byte[] Binary() => KbinConverter.ReadXmlBytes(_binary);

    [Benchmark]
    public byte[] Scalars() => KbinConverter.ReadXmlBytes(_scalars);
}
