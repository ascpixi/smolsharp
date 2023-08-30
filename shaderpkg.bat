@echo off
"./tools/shader_minifier.exe" -o minified.glsl --format text --smoothstep --aggressive-inlining %1
"./src/ShaderCompressor/bin/x64/Release/ShaderCompressor.exe" minified.glsl compressed.txt
