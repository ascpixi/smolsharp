using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32
{
    internal static unsafe class Kernel32
    {
        [SuppressGCTransition, DllImport("kernel32")]
        public static extern nint CreateThread(
            nint threadAttributes,
            nint stackSize,
            delegate* unmanaged<nint, uint> lpStartAddress,
            nint lpParameter = 0,
            uint creationFlags = 0,
            nint lpThreadId = 0
        );

        [SuppressGCTransition, DllImport("kernel32")]
        public static extern void* GlobalAlloc(uint dwFlags, nint dwBytes);

        [SuppressGCTransition, DllImport("kernel32")]
        public static extern bool AllocConsole();

        [SuppressGCTransition, DllImport("kernel32")]
        public static extern void Sleep(uint ms);
    }
}
