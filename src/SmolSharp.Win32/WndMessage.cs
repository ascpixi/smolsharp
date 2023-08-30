using System;
using System.Runtime.InteropServices;
using SmolSharp.Win32.GDI;

namespace SmolSharp.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WndMessage
    {
        public nint Hwnd;
        public uint Message;
        public nuint WParam;
        public nint LParam;
        public int Time;
        public GDIPoint Point;
        private readonly int lPrivate;
    }
}
