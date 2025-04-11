using dotnetCampus.Cli.Compiler;

namespace dotnetCampus.Cli.Tests.Fakes;

public class PrimaryOptions
{
    [Option('a', "Byte")]
    public byte Aaa { get; set; }

    [Option('b', "Int16")]
    public short Bbb { get; set; }

    [Option('c', "UInt16")]
    public ushort Ccc { get; set; }

    [Option('d', "Int32")]
    public int Ddd { get; set; }

    [Option('e', "UInt32")]
    public uint Eee { get; set; }

    [Option('f', "Int64")]
    public long Fff { get; set; }

    [Option('g', "UInt64")]
    public ulong Ggg { get; set; }

    [Option('h', "Single")]
    public float Hhh { get; set; }

    [Option('i', "Double")]
    public double Iii { get; set; }

    [Option('j', "Decimal")]
    public decimal Jjj { get; set; }
}
