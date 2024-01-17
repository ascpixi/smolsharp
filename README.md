
# 🐜 SmolSharp
SmolSharp is a repository that demonstrates the ability to use NativeAOT to build extremely small binaries without any kind of external utility or linker. For example, for a simple hello world program, by default, NAOT produces a binary that is `2998272` bytes in size with the following properties:
```xml
<PublishAot>true</PublishAot>
<Optimize>true</Optimize>
<OptimizationPreference>Size</OptimizationPreference>
<PublishTrimmed>true</PublishTrimmed>
```
With the `SmolSharp.props` file being imported, the compiler produces a binary that is only `2021` bytes in size - a 0.07% of the original file-size.

## Project overview
| Project Name     | Binary size | Description                                                        |
| ---------------- | ----------- | ------------------------------------------------------------------ |
| HelloWorld       | 2021 B      | A console program that outputs "Hello World".
| Mandelbrot       | 2879 B      | A windowed program that renders a fractal (the Mandelbrot set).
| Ocean            | 7316 B      | A windowed OpenGL program that renders a ray-marched stylized ocean.

As of pull-request [Compile as x86](https://github.com/ascpixi/smolsharp/pull/3) by [Michal Strehovský](https://github.com/MichalStrehovsky), the projects may also be compiled in 32-bit mode, which reduces file sizes even further:

| Project Name     | Binary size | Description                                                        |
| ---------------- | ----------- | ------------------------------------------------------------------ |
| HelloWorld       | 1711 B      | A console program that outputs "Hello World".
| Mandelbrot       | 2299 B      | A windowed program that renders a fractal (the Mandelbrot set).
| Ocean            | 5832 B      | A windowed OpenGL program that renders a ray-marched stylized ocean.

You may find this version in the [`/bit32`](https://github.com/ascpixi/smolsharp/tree/bit32) branch.

https://github.com/ascpixi/smolsharp/assets/44982772/c70e1e20-7cef-473d-bbf5-67079bec2487
###### Screen capture of the Ocean demo

## Inner-workings
All of the functionality of SmolSharp is contained in the `SmolSharp.props` file. The following techniques are employed in order to achieve minimal binary sizes:
1.  **Custom standard library** - SmolSharp uses the [bflat zerolib](https://github.com/bflattened/bflat/tree/master/src/zerolib) standard library, serving as the primary size-saving technique. However, this results in the lack of any kind of GC and removes all built-in BCL classes and functionality, requiring the use of raw P/Invokes to interface with Windows' APIs.
2. **Raw P/Invokes** - all external `[DllImport]` declarations are specified in the `<DirectPInvoke>` list in the MSBuild `.props` file, removing the need for a dynamic loader. To prevent redundant `RhpReversePInvoke` calls, every `[DllImport]` is marked with the `[SuppressGCTransition]` attribute.
3. **ILC configuration** - several MSBuild properties instruct the IL compiler (ILC) to optimize and generate code with binary size as its top priority. All Win32 resources (usually embedded in the `.rsrc` section) are omitted by setting the internal property `_Win32ResFile` to an empty string, in a target that executes before the `LinkNative` target (or for .NET 8+, by setting an undocumented property).
4. **Native object file manipulation** - the alignment of all sections in the native object file is set to their minimum accepted value using `objcopy`. Additionally, since no exception handling is used, the SEH exception data directory (the `.pdata` section) is removed.
5. **Linker flags** - several MSVC linker flags are specified, significantly reducing the size of the final binary image:
	- `/align:16` - sets section alignment to 16 bytes, which, based on testing, is the minimum accepted value
	- `/manifestuac:no` - forces the linker to never embed any UAC manifest
	- `/opt:ref /opt:icf` - enables linker reference optimization
	- `/safeseh:no` - allows the linker to skip embedding SEH data
	- `/emittoolversioninfo:no` - removes linker/compiler version information (the Rich header). **Undocumented.**
	- `/emitpogophaseinfo` - removes the debug directory from the final output. **Undocumented.**
	- `/nodefaultlib` - excludes CRT libraries from the binary
	- `/fixed` - instructs the operating system to load the binary at a static address, disabling relocations and making the linker skip emitting the `.reloc` section
	- `/merge:.modules=.rdata` - merges the `.modules` and `.rdata` sections due to their identical attributes
	- `/merge:.managedcode=.text` - merges the `.managedcode` and `.text` sections due to their identical attributes
6. **Finishing touches** - all trailing null bytes are stripped from the binary.

Please note that these steps also may be performed without the use of MSBuild - this is demonstrated by the [`MichalStrehovsky/zerosharp`](https://github.com/MichalStrehovsky/zerosharp) repository.

## Caveats
As mentioned in the [Inner-workings](#Inner-workings) section, the lack of a GC means that object allocations are frowned upon, and all memory retrieved via dynamic allocation should be disposed of manually, similarly to C. As all of the BCL classes are missing, this also means that they have to be either re-implemented, or alternatives need to be used, like the host OS's built-in APIs. This project also only works on Windows - it depends on importing Win32 classes, and assumes the output binary format is PE, which is only majorly supported by NT-based OS's.

## Potential improvements
As this repository focuses on avoiding any kind of external tools, the default MSVC linker was used. However, specialized linkers such as the [Crinkler](https://github.com/runestubbe/Crinkler) may be used in order to compress the whole binary and avoid any unnecessary sections. 

## Building
In order to build any given project, in the project's root folder (near the .csproj file), simply run:
```console
dotnet publish -r win-x64 -c release
```

For the OpenGL ocean demo, you can quickly compress a GLSL fragment shader by using the `ShaderCompressor` project, included with the repository. The shader compressor is a simple C++ program that uses Windows's built-in cabinet compression API in order to create byte arrays that can then be consumed by the compiled application. It's recommended to also minify your shader - for example, with [`laurentlb/Shader_Minifier`](https://github.com/laurentlb/Shader_Minifier). The `shaderpkg` batch file will use the `shader_minifier` binary in the `./tools` directory. The main fragment shader for `SmolSharp.Ocean` is located in `./src/SmolSharp.Ocean/Shaders/frag.glsl`.
