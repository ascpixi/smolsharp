using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Ocean
{
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Buffer32 {
        public const int Size = 32;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct Buffer24 {
        public const int Size = 24;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8192)]
    public struct Buffer8192 {
        public const int Size = 8192;
    }
}
