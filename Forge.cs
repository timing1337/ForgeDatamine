using System.Numerics;
using System.Reflection.PortableExecutable;

namespace ForgeDatamine;

public class Forge : IDisposable {
    private readonly BinaryReader _reader;
    private readonly string _path;

    public List<FatDescriptor> Descriptors = new();
    public Dictionary<ulong, FatFile> Files = new();

    public uint Version;
    public long PosFat;
    public long GlobalMetaKey;
    public byte Flag;

    private Forge(BinaryReader reader, string path) {
        _reader = reader;
        _path = path;

        ParseMetadata();
        BuildFatDescriptors();
        IndexFatFiles();
    }

    private void ParseMetadata() {
        var header = _reader.ReadBytes(9);
        Version = _reader.ReadUInt32();
        PosFat = _reader.ReadInt64();
        GlobalMetaKey = _reader.ReadInt64();
        Flag = _reader.ReadByte();
    }

    private void BuildFatDescriptors() {
        _reader.BaseStream.Seek(PosFat, SeekOrigin.Begin);
        var maxFile = _reader.ReadUInt32();
        var maxDir = _reader.ReadUInt32();
        var maxKey = _reader.ReadUInt64();
        var root = _reader.ReadUInt32();
        var firstFreeFile = _reader.ReadUInt32();
        var firstFreeDir = _reader.ReadUInt32();
        var sizeofFat = _reader.ReadUInt32();
        var nbFat = _reader.ReadUInt32();
        var positionFatDescriptors = _reader.ReadInt64();

        _reader.BaseStream.Seek(positionFatDescriptors, SeekOrigin.Begin);
        for (int i = 0; i < nbFat; i++) {
            var descriptor = FatDescriptor.Read(_reader);
            if (descriptor.nextPosFat != -1) {
                _reader.BaseStream.Seek(descriptor.nextPosFat, SeekOrigin.Begin);
            }
            Descriptors.Add(descriptor);
        }
    }

    private void IndexFatFiles() {
        foreach (var desc in Descriptors) {
            _reader.BaseStream.Seek((long)desc.posFat, SeekOrigin.Begin);
            for (int i = 0; i < desc.maxFile; i++) {
                var file = FatFile.Read(_reader);

                // Decrypt
                if (Version >= 31) {
                    file.uid = BitOperations.RotateRight(file.uid + 0xAFADDBC7C7BBBC9C, (int)(file.offset % 62) + 1) ^ 0x3934394E23482361;
                }

                // Ignore metadata file, they are utterly useless anyways
                // 16 = globalMetaKey, 3040 = "hash" file?
                if (file.uid == (ulong)GlobalMetaKey || file.uid == 3040) {
                    continue;
                }

                Console.WriteLine("Indexed file: UID = {0}, Offset = {1}, Size = {2}", file.uid, file.offset, file.size);

                Files[file.uid] = file;
            }
        }
    }

    public static Forge Read(string path) {
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var forge = new Forge(new BinaryReader(stream), path);
        return forge;
    }

    public FatFile GetFatFile(ulong uid) {
        if (Files.TryGetValue(uid, out var file)) {
            return file;
        }
        throw new Exception($"File with UID {uid} not found.");
    }
    public Container GetContainer(ulong uid) {
        var file = GetFatFile(uid);
        _reader.BaseStream.Seek((long)file.offset, SeekOrigin.Begin);
        return Container.Read(_reader);
    }

    public void Dispose() {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }
}
