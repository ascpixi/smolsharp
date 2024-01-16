using SmolSharp.Win32.GDI.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32.GDI
{
    internal static unsafe class Gdi32
    {
        [SuppressGCTransition, DllImport("gdi32", EntryPoint = "_SetPixel@16")]
        public static extern uint SetPixel(
            nint hdc,
            int x, int y,
            uint color
        );

        [SuppressGCTransition, DllImport("gdi32", EntryPoint = "_ChoosePixelFormat@8")]
        public static extern int ChoosePixelFormat(
            nint hdc,
            PixelFormatDescriptor* ppfd
        );

        [SuppressGCTransition, DllImport("gdi32", EntryPoint = "_SetPixelFormat@12")]
        public static extern bool SetPixelFormat(
            nint hdc,
            int format,
            PixelFormatDescriptor* ppfd
        );

        [SuppressGCTransition, DllImport("gdi32", EntryPoint = "_SwapBuffers@4")]
        public static extern bool SwapBuffers(
            nint hdc
        );
    }
}
