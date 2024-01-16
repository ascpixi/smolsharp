using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32.GDI.OpenGL
{
    internal static class GL
    {
        public const uint COLOR_BUFFER_BIT = 0x00004000;
        public const uint FRAGMENT_SHADER = 35632;
        public const uint VERTEX_SHADER = 35633;
        public const uint ARRAY_BUFFER = 34962;
        public const uint STATIC_DRAW = 35044;
        public const uint FLOAT = 0x1406;
        public const uint FALSE = 0;
        public const uint TRIANGLES = 0x4;

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "_wglCreateContext@4")]
        public static extern nint CreateContext(nint hdc);

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "_wglMakeCurrent@8")]
        public static extern bool MakeCurrent(nint hdc, nint ctx);

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "glClearColor")]
        public static extern void SetClearColor(float r, float g, float b, float a);

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "glClear")]
        public static extern void Clear(uint mask);

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "_wglGetProcAddress@4")]
        public static unsafe extern void* GetProcAddress(byte* name);

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "_glDrawArrays@12")]
        public static extern void DrawArrays(
            uint mode,
            int first,
            int count
        );

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "_glViewport@16")]
        public static extern void Viewport(int x, int y, uint width, uint height);

        [SuppressGCTransition]
        [DllImport("opengl32", EntryPoint = "glGetError")]
        public static extern uint GetError();
    }
}
