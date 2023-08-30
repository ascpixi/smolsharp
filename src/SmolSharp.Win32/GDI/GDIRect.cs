using System.Runtime.InteropServices;

namespace SmolSharp.Win32.GDI
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GDIRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
