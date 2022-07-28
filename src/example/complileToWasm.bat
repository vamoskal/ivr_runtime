@echo off



set clang=%USERPROFILE%\.wasi-sdk\wasi-sdk-12.0\bin\clang.exe
set wasisdk=%USERPROFILE%\.wasi-sdk\wasi-sdk-12.0
set sysRoot=%USERPROFILE%\.nuget\packages\wasi.sdk\0.1.1\packs\wasi-wasm
set input=%~dp0sample.c
set output=%~dp0bin\sample.wasm
set outputWat=%~dp0bin\sample.wat
set wasm2wasi=%~dp0..\..\external\bin\wasm2wat.exe

set args="%input%" --sysroot="%wasisdk%\share\wasi-sysroot" -I "%sysRoot%\native\include" -o "%output%" -Wl,-z,stack-size=1048576,--initial-memory=524288000,--max-memory=524288000

echo "%clang%" %args%
"%clang%" %args%
"%wasm2wasi%" "%output%" -o "%outputWat%"
