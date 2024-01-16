using SmolSharp.Win32.GDI;
using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Win32
{
    internal static unsafe class User32
    {
        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "_CreateWindowExA@48")]
        public static extern nint CreateWindowEx(
            uint dwExStyles,
            void* lpClassName,
            void* lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            nint hwndParent = 0,
            nint hMenu = 0,
            nint hInstance = 0,
            nint lpParam = 0
        );

        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "_RegisterClassA@4")]
        public static extern nint RegisterClass(WindowClassA* lpWndClass);

        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "_ShowWindow@8")]
        public static extern nint ShowWindow(nint hWnd, int nCmdShow);

        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "_GetMessageA@16")]
        public static extern int GetMessage(
            WndMessage* lpMsg,
            nint hwnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax
        );

        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "TranslateMessage")]
        public static extern bool TranslateMessage(WndMessage* lpMsg);

        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "_DispatchMessageA@4")]
        public static extern bool DispatchMessage(WndMessage* lpMsg);

        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "_DefWindowProcA@16")]
        public static extern nint DefWindowProc(
            nint hwnd,
            uint msg,
            nuint wParam,
            nint lParam
        );

        [SuppressGCTransition]
        [DllImport("user32", EntryPoint = "_GetDC@4")]
        public static extern nint GetDC(nint hwnd);

        //[DllImport("user32")]
        //public static extern nint BeginPaint(
        //    nint hWnd,
        //    GDIPaintStruct* lpPaint
        //);

        //[DllImport("user32")]
        //public static extern bool EndPaint(
        //    nint hWnd,
        //    GDIPaintStruct* lpPaint
        //);

        [SuppressGCTransition]
        [DllImport("user32")]
        public static extern bool GetWindowRect(
            nint hwnd,
            GDIRect* lpRect
        );

        [SuppressGCTransition]
        [DllImport("user32")]
        public static extern void PostQuitMessage(int nExitCode);
    }
}
