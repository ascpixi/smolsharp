using SmolSharp.Mandelbrot;
using SmolSharp.Win32;
using System;
using System.Runtime.InteropServices;

namespace SmolSharp.Raytracer
{
    internal static class Program
    {
        static void Main() { }

        [UnmanagedCallersOnly(EntryPoint = "smolsharp_main")]
        public static unsafe int UnmanagedMain(nint hInstance, nint hPrevInstance, char* pCmdLine, int nCmdShow)
        {
            var name = "Demo"u8.AsPointer();
            var wndClass = new WindowClassA() {
                WndProc = &WndProc,
                HInstance = hInstance,
                ClassName = name
            };

            User32.RegisterClass(&wndClass);

            nint hwnd = User32.CreateWindowEx(
                0, name, name,
                (0x00000000 | 0x00C00000 | 0x00080000 | /*0x00040000 |*/ 0x00020000 /*| 0x00010000*/),
                64, 64, 640, 480,
                0, 0, hInstance, 0
            );

            User32.ShowWindow(hwnd, 5);

            WndMessage msg = default;
            while (User32.GetMessage(&msg, default, 0, 0) > 0) {
                //User32.TranslateMessage(&msg);
                User32.DispatchMessage(&msg);
            }

            return 0;
        }

        [UnmanagedCallersOnly]
        unsafe static nint WndProc(nint hwnd, uint msg, nuint wParam, nint lParam)
        {
            switch (msg) {
                case 0xF: // WM_PAINT
                    //GDIPaintStruct ps = default;
                    //nint hdc = User32.BeginPaint(hwnd, &ps);
                    nint hdc = User32.GetDC(hwnd); // using the HDC directly saves us a couple of bytes
                    FractalRenderer.Render(hdc);
                    //User32.EndPaint(hdc, &ps);
                    break;
                case 0x10: // WM_DESTROY
                    *(byte*)0 = 1; // intentionally crash
                    //User32.PostQuitMessage(0);
                    break;
            }

            return User32.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }
}