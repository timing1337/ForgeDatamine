using System;
using System.Collections.Generic;
using System.Text;

namespace ForgeDatamine;

public class ContainerEntry {
    public int Size;
    public int Offset;
}

public class Container {

    public Dictionary<ulong, ContainerEntry> Entries = new();
    public CompressedBlock Body;
    public MemoryStream Data => Body.Data;

    public static Container Read(BinaryReader reader) {
        var header = CompressedBlock.Read(reader);
        var headerReader = new BinaryReader(header.Data);
        var entriesCount = headerReader.ReadUInt32();
        int offset = 0;
        var container = new Container();

        for (int i = 0; i < entriesCount; i++) {
            var uid = headerReader.ReadUInt64();
            var size = headerReader.ReadInt32();
            var entry = new ContainerEntry() {
                Size = size,
                Offset = offset
            };
            offset += size;
            container.Entries.Add(uid, entry);
        }

        container.Body = CompressedBlock.Read(reader);
        return container;
    }

    public byte[] ExtractFile(uint uid) {
        var entry = Entries[uid];
        var data = new byte[entry.Size];
        Data.Seek(entry.Offset, SeekOrigin.Begin);
        Data.ReadExactly(data, 0, entry.Size);
        return data;
    }
}
