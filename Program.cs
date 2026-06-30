using System.Runtime.InteropServices;
using System.Text;

namespace ForgeDatamine;

internal class Program
{

    static void Main(string[] args)
    {
        using var forge = Forge.Read(@"E:\Downloads\Y8S4_DeepFreeze\datapc64.forge");

        var container = forge.GetContainer(3624);
        foreach(var entry in container.Entries) {
            var content = container.ExtractFile(entry.Key);
            var span = content.AsSpan();
            var len = MemoryMarshal.Read<ushort>(span);
            var encryptedStr = span.Slice(2, len);
            var unknown = MemoryMarshal.Read<ushort>(span.Slice(2 + len));
            var dataLength = MemoryMarshal.Read<uint>(span.Slice(2 + len + 2));
            var classId = MemoryMarshal.Read<uint>(span.Slice(2 + len + 2 + 4));
            var uid = MemoryMarshal.Read<ulong>(span.Slice(2 + len + 2 + 4 + 4));
        }
    }
}
