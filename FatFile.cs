using System;
using System.Collections.Generic;
using System.Text;

namespace ForgeDatamine;

public class FatFile {
    public ulong offset;
    public ulong uid;
    public uint size;
    public ulong end;

    public static FatFile Read(BinaryReader reader) {
        var chunk = new FatFile {
            offset = reader.ReadUInt64(),
            uid = reader.ReadUInt64(),
            size = reader.ReadUInt32()
        };

        chunk.end = chunk.offset + chunk.size;
        return chunk;
    }
}
