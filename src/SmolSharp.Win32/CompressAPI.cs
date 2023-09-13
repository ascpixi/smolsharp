using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32
{
    internal static unsafe class CompressAPI
    {
        [SuppressGCTransition, DllImport("Cabinet", EntryPoint = "_CreateDecompressor@12")]
        public static extern bool CreateDecompressor(
            CompressAlgorithm algorithm,
            nint allocationRoutines,
            nint* decompressorHandle
        );

        [SuppressGCTransition, DllImport("Cabinet", EntryPoint = "_Decompress@24")]
        public static extern bool Decompress(
            nint decompressorHandle,
            void* compressedData,
            ulong compressedDataSize,
            void* uncompressedBuffer,
            nint uncompressedBufferSize,
            ulong* uncompressedDataSize
        );
    }

    public enum CompressAlgorithm : uint
    {
        MSZip = 2,
        Xpress = 3,
        XpressHuffman = 4,
        LZMS = 5
    }
}
