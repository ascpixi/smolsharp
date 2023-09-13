using SmolSharp.Win32.GDI.OpenGL;
using SmolSharp.Win32;
using System;
using System.Runtime.InteropServices;
using SmolSharp.Win32.GDI;
using SmolSharp.Ocean.Shaders;

namespace SmolSharp.Ocean
{
    internal class Program
    {
        static ushort windowWidth, windowHeight;
        static bool windowDimensionsChanged;

        static void Main() { }

        [UnmanagedCallersOnly(EntryPoint = "smolsharp_main")]
        public static unsafe int UnmanagedMain(nint hInstance, nint hPrevInstance, char* pCmdLine, int nCmdShow)
        {
            void* name = "Demo"u8.AsPointer();
            var wndClass = new WindowClassA() {
                WndProc = &WndProc,
                HInstance = hInstance,
                ClassName = name
            };

            User32.RegisterClass(&wndClass);

            nint hwnd = User32.CreateWindowEx(
                0, name, name,
                (0x00000000 | 0x00C00000 | 0x00080000 | 0x00040000 | 0x00020000 | 0x00010000),
                64, 64, 640, 480,
                0, 0, hInstance, 0
            );

            windowWidth = 640;
            windowHeight = 480;

            User32.ShowWindow(hwnd, 5);
            Kernel32.CreateThread(0, 0, &RenderThread, hwnd);

            WndMessage msg = default;
            while (User32.GetMessage(&msg, default, 0, 0) > 0) {
                User32.DispatchMessage(&msg);
            }

            return 0;
        }

        [UnmanagedCallersOnly]
        static unsafe uint RenderThread(nint hwnd)
        {
            var pfd = new PixelFormatDescriptor() {
                Size = 40,
                Version = 1,
                Flags = PFDFlags.DoubleBuffered | PFDFlags.DrawToWindow | PFDFlags.SupportOpenGL,
                // Type = PFD_TYPE_RGBA, // (0)
                ColorBits = 32,
                DepthBits = 24,
                StencilBits = 8
            };

            nint hdc = User32.GetDC(hwnd);
            int autoFormat = Gdi32.ChoosePixelFormat(hdc, &pfd);
            Gdi32.SetPixelFormat(hdc, autoFormat, &pfd);

            nint glCtx = GL.CreateContext(hdc);
            GL.MakeCurrent(hdc, glCtx);

            #region GL imports
            // Unfortunately, Windows's GDI only has GL1.1 methods defined...
            // We have to import them ourselves. For this, we use a decoder
            // that uses encoded tokens in order to reduce repetition.
            // This is only done to conserve space, and you could just as well
            // call GL.GetProcAddress directly.
            var glCreateShader = (delegate* unmanaged<uint, uint>)
                GLLoader.ImportFromBlob(0, 12);

            var glCompileShader = (delegate* unmanaged<uint, void>)
                GLLoader.ImportFromBlob(12, 13);

            var glAttachShader = (delegate* unmanaged<uint, uint, void>)
                GLLoader.ImportFromBlob(25, 12);

            var glShaderSource = (delegate* unmanaged<uint, int, byte**, int*, void>)
                GLLoader.ImportFromBlob(31, 12);

            var glCreateProgram = (delegate* unmanaged<uint>)
                GLLoader.ImportFromBlob(43, 13);

            var glLinkProgram = (delegate* unmanaged<uint, void>)
                GLLoader.ImportFromBlob(56, 11);

            var glUseProgram = (delegate* unmanaged<uint, void>)
                GLLoader.ImportFromBlob(67, 10);

            var glVertexAttribPointer = (delegate* unmanaged<uint, int, uint, byte, int, void*, void>)
                GLLoader.ImportFromBlob(77, 19);

            var glEnableVertexAttribArray = (delegate* unmanaged<uint, void>)
                GLLoader.ImportFromBlob(96, 23);

            var glGenVertexArrays = (delegate* unmanaged<int, uint*, void>)
                GLLoader.ImportFromBlob(119, 15);

            var glGenBuffers = (delegate* unmanaged<int, uint*, void>)
                GLLoader.ImportFromBlob(134, 10);

            var glBindVertexArray = (delegate* unmanaged<uint, void>)
                GLLoader.ImportFromBlob(144, 15);

            var glBindBuffer = (delegate* unmanaged<uint, uint, void>)
                GLLoader.ImportFromBlob(159, 10);

            var glBufferData = (delegate* unmanaged<uint, int, void*, uint, void>)
                GLLoader.ImportFromBlob(163, 10);

            var glGetUniformLocation = (delegate* unmanaged<uint, byte*, int>)
                GLLoader.ImportFromBlob(173, 18);

            var glUniform3f = (delegate* unmanaged<int, float, float, float, void>)
                GLLoader.ImportFromBlob(191, 9);

#if DEBUG
            var glGetShaderiv = (delegate* unmanaged<uint, uint, int*, void>)
                GLLoader.Import("GetShaderiv"u8);

            var glGetShaderInfoLog = (delegate* unmanaged<uint, uint, int*, byte*, void>)
                GLLoader.Import("GetShaderInfoLog"u8);

            var glGetProgramiv = (delegate* unmanaged<uint, uint, int*, void>)
                GLLoader.Import("GetProgramiv"u8);

            var glGetProgramInfoLog = (delegate* unmanaged<uint, uint, int*, byte*, void>)
                GLLoader.Import("GetProgramInfoLog"u8);
#endif
            #endregion

            // Initialize vertices, shaders, etc...
            uint vertexShader = glCreateShader(GL.VERTEX_SHADER);
            var vertexShaderCode = OceanShader.VertexShader();
            glShaderSource(vertexShader, 1, &vertexShaderCode, null);
            glCompileShader(vertexShader);

            var compressedFragShader = OceanShader.FragmentShader();
            var fragBuffer = Kernel32.GlobalAlloc(default, 8192);
            nint hDcmp;
            nint fragLength;
            bool success;

            success = CompressAPI.CreateDecompressor(CompressAlgorithm.MSZip, default, &hDcmp);
            success = CompressAPI.Decompress(
                hDcmp,
                compressedFragShader.AsPointer(),
                compressedFragShader.Length,
                fragBuffer,
                8192,
                &fragLength
            );

            uint fragShader = glCreateShader(GL.FRAGMENT_SHADER);
            glShaderSource(fragShader, 1, (byte**)&fragBuffer, (int*)&fragLength);
            glCompileShader(fragShader);

#if DEBUG
            int fragShaderCompiled = 0;
            glGetShaderiv(fragShader, 35713, &fragShaderCompiled);
            if (fragShaderCompiled == 0) {
                byte* fragLog = (byte*)Kernel32.GlobalAlloc(0, 4096);
                glGetShaderInfoLog(fragShader, 4096, null, fragLog);
                ReportError("Couldn't compile the fragment shader!\n\n", fragLog);
            }
#endif

            uint shaderProgram = glCreateProgram();
            glAttachShader(shaderProgram, vertexShader);
            glAttachShader(shaderProgram, fragShader);
            glLinkProgram(shaderProgram);

#if DEBUG
            int shaderLinked = 0;
            glGetProgramiv(shaderProgram, 35714, &shaderLinked);
            if (shaderLinked == 0) {
                byte* linkLog = (byte*)Kernel32.GlobalAlloc(0, 4096);
                glGetProgramInfoLog(shaderProgram, 4096, null, linkLog);
                ReportError("Couldn't link the shader program!\n\n", linkLog);
            }
#endif

            // We use a clipped full-screen triangle in order to render
            // the fragment shader to the whole screen. This is usually
            // faster than a quad (see https://wallisc.github.io/rendering/2021/04/18/Fullscreen-Pass.html)
            // and shaves a couple of bytes of the final binary.
            var buffer = new Buffer24();
            float* v = (float*)&buffer;
            v[0] = -1.0f; v[1] =  3.0f;
            v[2] = -1.0f; v[3] = -1.0f;
            v[4] =  3.0f; v[5] = -1.0f;

            uint vbo = 0, vao = 0;
            glGenVertexArrays(1, &vao);
            glGenBuffers(1, &vbo);

            glBindVertexArray(vao);
            glBindBuffer(GL.ARRAY_BUFFER, vbo);
            glBufferData(GL.ARRAY_BUFFER, 24, v, GL.STATIC_DRAW);

            glVertexAttribPointer(0, 2, GL.FLOAT, 0, 2 * sizeof(float), null);
            glEnableVertexAttribArray(0);

            uint begin = WinMM.GetTime();

            do {
                //GL.SetClearColor(0f, 0f, 0f, 1f);
                //GL.Clear(GL.COLOR_BUFFER_BIT);

                uint timeMs = WinMM.GetTime() - begin;
                float time = timeMs / 1000f;

                if (windowDimensionsChanged) {
                    GL.Viewport(0, 0, windowWidth, windowHeight);
                    windowDimensionsChanged = false;
                }

                int location = glGetUniformLocation(shaderProgram, "s"u8.AsPointer());
                glUseProgram(shaderProgram);
                glUniform3f(
                    location,
                    windowWidth, windowHeight,
                    time
                );

                GL.DrawArrays(GL.TRIANGLES, 0, 3);

                Gdi32.SwapBuffers(hdc);
            } while (true);
        }

#if DEBUG
        unsafe static void ReportError(string error, byte* log)
        {
            Kernel32.AllocConsole();
            Console.WriteLine(error);

            byte c;
            while ((c = *log++) != 0x00)
                Console.Write((char)c);

            Console.WriteLine("\n\n(end)");
            Kernel32.Sleep(10000);
        }
#endif

        [UnmanagedCallersOnly]
        unsafe static nint WndProc(nint hwnd, uint msg, nuint wParam, nint lParam)
        {
            switch (msg) {
                case 0x10: // WM_DESTROY
                    *(byte*)0 = 1; // intentionally crash
                    break;
                case 0x05: // WM_SIZE
                    windowWidth = (ushort)(lParam & 0xFFFF);
                    windowHeight = (ushort)(lParam >> 16);
                    windowDimensionsChanged = true;
                    break;
            }

            return User32.DefWindowProc(hwnd, msg, wParam, lParam);
        }
    }
}