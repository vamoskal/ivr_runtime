module Generator

open System
open System.IO
open System.Diagnostics
open System.Threading.Tasks

type Lang=
| CLang
| AssemblyScript

let currentDir = FileInfo(Reflection.Assembly.GetCallingAssembly().Location).DirectoryName

module private Constants=
    let tempFolder = sprintf "%s\\WebRoot" <| currentDir
    let userProfile = Environment.GetEnvironmentVariable("USERPROFILE")

    let clang = sprintf "%s\\.wasi-sdk\\wasi-sdk-12.0\\bin\\clang.exe" userProfile
    let wasisdk = sprintf "%s\\.wasi-sdk\\wasi-sdk-12.0" userProfile
    let includes = sprintf "%s\\.nuget\\packages\\wasi.sdk\\0.1.1\\packs\\wasi-wasm" userProfile
    
    let getInFilePath ext id =
        sprintf "%s\\%d.%s" tempFolder id ext
        
    let getInFilePathClang = getInFilePath "c"
    let getInFilePathAs = getInFilePath "ts"

    let getOutFilePath id=
        sprintf "%s\\%d.wasm" tempFolder id

    let arguments = sprintf "--sysroot=\"%s\\share\\wasi-sysroot\" -I \"%s\\native\\include\" \"-Wl,-z,stack-size=1048576,--initial-memory=524288000,--max-memory=524288000\"" wasisdk includes 
    let assemblyScriptWorkindDir = sprintf "%s/../../../../assemblyScript/" currentDir
    let getCmdArgs lang inFile outFile=
        match lang with
        | CLang -> 
            (clang),(sprintf "\"%s\" -o \"%s\" %s" inFile outFile arguments),currentDir
        | AssemblyScript -> 
            ("cmd.exe"),(sprintf "/c npx asc \"%s\" -o \"%s\"" inFile outFile), assemblyScriptWorkindDir
        
        


let tryFindById id=
    let path = Constants.getOutFilePath id
    File.Exists(path), path
    
let private runCmd(executable, args, workingDir)=  
    task{
        use p = new Process()
        p.StartInfo.FileName <- executable
        p.StartInfo.Arguments <- args
        p.StartInfo.WorkingDirectory <- workingDir
        p.StartInfo.CreateNoWindow <- true
        //p.StartInfo.UseShellExecute <- true
        //p.StartInfo.CreateNoWindow <- false
        //p.StartInfo.WindowStyle <- ProcessWindowStyle.Normal
        //p.StartInfo.RedirectStandardError <- true
        //p.StartInfo.RedirectStandardOutput <- true
        p.Start() |> ignore
        
        do! p.WaitForExitAsync()
        //let! oputput = p.StandardOutput.ReadToEndAsync()
        //let! err = p.StandardError.ReadToEndAsync()
        ignore()
    }
    
let generateWasm id text (lang:Lang)=
    task{
        let inPath = match lang with
                     | CLang -> Constants.getInFilePathClang id 
                     | AssemblyScript -> Constants.getInFilePathAs id 

        let outPath = Constants.getOutFilePath id

        let cmdInput = Constants.getCmdArgs lang inPath outPath

                
        do! File.WriteAllTextAsync(inPath , text)
        do! runCmd cmdInput
        
        return outPath
    }
