#define USE_ASCII
using System;
using System.Runtime.InteropServices;

namespace SmolSharp
{
    internal class Program
    {
        // dummy method
        static void Main() { }

        [UnmanagedCallersOnly(EntryPoint = "smolsharp_main")]
        unsafe static void UnmanagedMain()
        {
#if USE_ASCII
            var hi = "Hello World!"u8;
            for (int i = 0; i < hi.Length; i++) {
                Console.Write((char)hi[i]);
            }
#else
            Console.WriteLine("Hello World!");
#endif
        }
    }
}