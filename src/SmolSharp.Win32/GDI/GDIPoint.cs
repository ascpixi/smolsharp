using System.Runtime.InteropServices;

namespace SmolSharp.Win32.GDI
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GDIPoint
    {
        public int X;
        public int Y;
    }
}
