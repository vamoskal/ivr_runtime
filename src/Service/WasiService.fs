module WasiService

open System
open Wasmtime
    

type Runtime(moduleNamespace: string)=
    let engine = new Engine();
    let linker = new Linker(engine)
    let store = new Store(engine)
    do
        let config = WasiConfiguration()
        store.SetWasiConfiguration(config)

    let printExitCode:Action<int> = new Action<int>(fun exitCode -> printf "%d" exitCode)

    interface IDisposable with
        member this.Dispose()=
            store.Dispose()
            linker.Dispose()
            engine.Dispose()

    member this.Init()=
        linker.DefineWasi()
        linker.Define("env", "log_external", Function.FromCallback(store, Action<int>(fun n-> printfn "%d" n)))
    
    member this.Execute(file:string)=
        use wasmModule = Module.FromFile(engine, file)
        let instance = linker.Instantiate(store, wasmModule)
        let run = instance.GetAction(store, "_start")
        run.Invoke()