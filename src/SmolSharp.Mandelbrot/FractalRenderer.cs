using SmolSharp.Win32.GDI;

namespace SmolSharp.Mandelbrot
{
    internal static class FractalRenderer
    {
        const float OffsetX = 0;
        const float OffsetY = 0;
        const int Iterations = 15;
        const float Zoom = 4f;

        public static void Render(nint hdc)
        {
            for (int x = 0; x < 640; x++) {
                for (int y = 0; y < 480; y++) {
                    float a = x / 640f;
                    float b = y / 480f;

                    // Taken from my GLSL implementation @ https://www.shadertoy.com/view/dlG3RR
                    a = (a - 0.5f) * Zoom + OffsetX;
                    b = (b - 0.5f) * Zoom * (480f / 640f) + OffsetY;

                    float c = 0.0f; // current real
                    float d = 0.0f; // current imaginary

                    for (int n = 0; n < Iterations; n++) {
                        float re = c * c - d * d + a;
                        float im = 2.0f * c * d + b;
                        c = re;
                        d = im;
                    }

                    uint r = (byte)(255 * c);
                    uint g = (byte)(255 * d);
                    uint color = (r) | (g << 8);
                    Gdi32.SetPixel(hdc, x, y, color);
                }
            }
        }
    }
}
