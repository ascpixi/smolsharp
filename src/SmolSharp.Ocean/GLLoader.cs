using BFlat.ZeroLib;
using SmolSharp.Win32;
using SmolSharp.Win32.GDI.OpenGL;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SmolSharp.Ocean
{
    internal static unsafe class GLLoader
    {
        static ReadOnlySpan<byte> SymbolBlob()
        {
            return "CreateShaderCompileShaderAttachShaderSourceCreateProgramLinkProgramUseProgramVertexAttribPointerEnableVertexAttribArrayGenVertexArraysGenBuffersBindVertexArrayBindBufferDataGetUniformLocationUniform3f"u8;
        }

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct Buffer32 { }

        public static void* ImportFromBlob(int idx, int length)
        {
            return Import(SymbolBlob().AsPointer() + idx, length);
        }

        public static void* Import(byte* name, int nameLength)
        {
            var buffer = new Buffer32();
            byte* str = (byte*)&buffer;

            str[0] = (byte)'g';
            str[1] = (byte)'l';

            for (int i = 0; i < nameLength; i++)
                str[i + 2] = name[i];

            return GL.GetProcAddress(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Import(ReadOnlySpan<byte> name)
            => Import(name.AsPointer(), name.Length);
    }
}
