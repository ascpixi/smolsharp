using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WindowClassA
    {
        public uint Style;
        public delegate* unmanaged<nint, uint, nuint, nint, nint> WndProc;
        public int ClsExtra;
        public int WndExtra;
        public nint HInstance;
        public nint HIcon;
        public nint HCursor;
        public nint HBrBackground;
        public void* MenuName;
        public void* ClassName;
    }
}
