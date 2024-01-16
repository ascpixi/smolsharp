using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32
{
    internal static class WinMM
    {
        [SuppressGCTransition]
        [DllImport("winmm.dll", EntryPoint = "_timeGetTime@0")]
        public static extern uint GetTime();
    }
}
