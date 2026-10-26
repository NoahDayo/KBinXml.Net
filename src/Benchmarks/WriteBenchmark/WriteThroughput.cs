using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using KbinXml.Net;

namespace WriteBenchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class WriteThroughput
{
    private byte[] _mixed = null!;
    private byte[] _binary = null!;
    private byte[] _scalars = null!;

    [GlobalSetup]
    public void Setup()
    {
        _mixed = Encoding.UTF8.GetBytes(File.ReadAllText("data/small.xml"));

        var binaryXml = new StringBuilder("<root>");
        var hex = new string('a', 8192);
        for (var i = 0; i < 16; i++)
            binaryXml.Append("<data __type=\"bin\">").Append(hex).Append("</data>");
        binaryXml.Append("</root>");
        _binary = Encoding.UTF8.GetBytes(binaryXml.ToString());

        var scalarXml = new StringBuilder("<root>");
        for (var i = 0; i < 1000; i++)
            scalarXml.Append("<value __type=\"s32\">12345</value>");
        scalarXml.Append("</root>");
        _scalars = Encoding.UTF8.GetBytes(scalarXml.ToString());
    }

    [Benchmark]
    public byte[] Mixed() => KbinConverter.Write(_mixed, KnownEncodings.UTF8);

    [Benchmark]
    public byte[] Binary() => KbinConverter.Write(_binary, KnownEncodings.UTF8);

    [Benchmark]
    public byte[] Scalars() => KbinConverter.Write(_scalars, KnownEncodings.UTF8);
}
