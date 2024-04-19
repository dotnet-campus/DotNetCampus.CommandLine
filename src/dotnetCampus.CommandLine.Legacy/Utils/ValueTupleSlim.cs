using System.Runtime.InteropServices;

namespace dotnetCampus.Cli.Utils
{
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct ValueTupleSlim<T1, T2>
    {
        public ValueTupleSlim(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { get; }
        public T2 Item2 { get; }

        public void Deconstruct(out T1 item1, out T2 item2)
        {
            item1 = Item1;
            item2 = Item2;
        }
    }
}
