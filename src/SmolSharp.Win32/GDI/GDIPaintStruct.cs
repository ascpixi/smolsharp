using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32.GDI
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct GDIPaintStruct
    {
        public nint Hdc;
        public int Erase; // BOOL
        public GDIRect RcPaint;
        public int Restore; // BOOL
        public int IncUpdate; // BOOL
        public fixed byte RgbReserved[32];
    }
}
