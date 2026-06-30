namespace ForgeDatamine;

public class ChunkedData {
    public uint compressedLength;
    public uint decompressedLength;
    public uint checksum;

    public static ChunkedData Read(BinaryReader reader) {
        return new ChunkedData {
            decompressedLength = reader.ReadUInt32(),
            compressedLength = reader.ReadUInt32()
        };
    }
}

public class CompressedBlock {
    public MemoryStream Data = new();

    public static CompressedBlock Read(BinaryReader reader) {
        var headerMagic = reader.ReadUInt64();
        if (headerMagic != 0x1015FA9957FBAA37) {
            throw new Exception("Unsupported compressed block format");
        }
        var version = reader.ReadUInt16();
        if (version >= 4) {
            throw new Exception("Unsupported compressed block version");
        }
        var compressionAlgo = reader.ReadByte();
        if (compressionAlgo >= 17) {
            throw new Exception("Unsupported compression algorithm");
        }

        // "Block size" ?
        reader.ReadInt32();

        var numChunks = reader.ReadInt32();
        var chunks = new ChunkedData[numChunks];

        long totalDecompressed = 0;
        for (var i = 0; i < numChunks; i++) {
            chunks[i] = ChunkedData.Read(reader);
            totalDecompressed += chunks[i].decompressedLength;
        }

        Console.WriteLine($"Decompressing {numChunks} chunks, total decompressed size: {totalDecompressed} bytes");

        var block = new CompressedBlock {
            Data = new MemoryStream((int)totalDecompressed)
        };

        foreach (var chunk in chunks) {
            chunk.checksum = reader.ReadUInt32();

            var compressed = reader.ReadBytes((int)chunk.compressedLength);
            if (chunk.compressedLength != chunk.decompressedLength) {
                var raw = new byte[chunk.decompressedLength];
                var result = Oodle.Decompress(compressed, raw);
                if (result != chunk.decompressedLength) {
                    throw new Exception("Decompression failed");
                }
                block.Data.Write(raw, 0, raw.Length);
            } else {
                block.Data.Write(compressed, 0, compressed.Length);
            }
        }

        return block;
    }
}
