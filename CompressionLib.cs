using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ZstdSharp;

namespace ForgeDatamine;

public static unsafe class Oodle {
    [DllImport("Dependencies/oo2core_8_win64.dll")]
    public static extern long OodleLZ_Decompress(
       byte* compBuf, int compSize, byte* rawBuf, int rawSize,
       int fuzzSafe,
       int checkCrc,
       int verbosity,
       byte* dictBuff,
       ulong dictSize,
       ulong unk,
       void* unkCallback,
       byte* scratchBuff,
       ulong scratchSize,
       int threadPhase);

    public static long Decompress(byte[] compressedData, byte[] decompressedData) {
        fixed (byte* compPtr = compressedData)
        fixed (byte* rawPtr = decompressedData) {
            return OodleLZ_Decompress(
                compPtr,
                compressedData.Length,
                rawPtr,
                decompressedData.Length,
                1, 0, 0, null, 0, 0, null, null, 0, 3);
        }
    }
}


public static class Zstd {
    private static readonly Decompressor _decompressor = new();

    public static byte[] Decompress(byte[] compressed) {
        return _decompressor.Unwrap(compressed).ToArray();
    }
}
