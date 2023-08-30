using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace BFlat.ZeroLib
{
    internal unsafe static class UnmanagedExports
    {
        [RuntimeExport("memcpy")]
        public static void Memcpy(byte* dest, byte* src, ulong num)
        {
            for (ulong i = 0; i < num; i++) {
                dest[i] = src[i];
            }
        }
    }
}
