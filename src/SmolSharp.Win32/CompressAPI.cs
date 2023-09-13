using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32
{
    internal static unsafe class CompressAPI
    {
        [SuppressGCTransition, DllImport("Cabinet")]
        public static extern bool CreateDecompressor(
            CompressAlgorithm algorithm,
            nint allocationRoutines,
            nint* decompressorHandle
        );

        [SuppressGCTransition, DllImport("Cabinet")]
        public static extern bool Decompress(
            nint decompressorHandle,
            void* compressedData,
            nint compressedDataSize,
            void* uncompressedBuffer,
            nint uncompressedBufferSize,
            nint* uncompressedDataSize
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
