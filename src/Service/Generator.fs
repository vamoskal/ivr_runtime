module Generator

open System
open System.IO
open System.Diagnostics
open System.Threading.Tasks

let currentDir = FileInfo(Reflection.Assembly.GetCallingAssembly().Location).DirectoryName
module private Constants=
    let tempFolder = sprintf "%s\\WebRoot" <| currentDir
    let userProfile = Environment.GetEnvironmentVariable("USERPROFILE")

    let clang = sprintf "%s\\.wasi-sdk\\wasi-sdk-12.0\\bin\\clang.exe" userProfile
    let wasisdk = sprintf "%s\\.wasi-sdk\\wasi-sdk-12.0" userProfile
    let includes = sprintf "%s\\.nuget\\packages\\wasi.sdk\\0.1.1\\packs\\wasi-wasm" userProfile
    
    let getInFilePath id=
        sprintf "%s\\%d.c" tempFolder id
    let getOutFilePath id=
        sprintf "%s\\%d.wasm" tempFolder id

    let arguments = sprintf "--sysroot=\"%s\\share\\wasi-sysroot\" -I \"%s\\native\\include\" \"-Wl,-z,stack-size=1048576,--initial-memory=524288000,--max-memory=524288000\"" wasisdk includes 

let tryFindById id=
    let path = Constants.getOutFilePath id
    File.Exists(path), path
    

let generateWasm id text= 
    task{
        let inPath = Constants.getInFilePath id
        let outPath = Constants.getOutFilePath id

        do! IO.File.WriteAllTextAsync(inPath, text)

        use p = new Process()
        p.StartInfo.FileName <- Constants.clang
        p.StartInfo.Arguments <- sprintf "\"%s\" -o \"%s\" %s" inPath outPath Constants.arguments
        p.StartInfo.CreateNoWindow <- false
        p.StartInfo.WindowStyle <- ProcessWindowStyle.Normal
        p.StartInfo.RedirectStandardError <- true
        p.StartInfo.RedirectStandardOutput <- true
        p.Start() |> ignore
        
        do! p.WaitForExitAsync()
        let! oputput = p.StandardOutput.ReadToEndAsync()
        let! err = p.StandardError.ReadToEndAsync()
        return outPath
    }