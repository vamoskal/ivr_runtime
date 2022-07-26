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

    let log msg = 
        printfn "%s" msg

    let promptAndFill(prompt:string)=
        printfn "You said '%s'" prompt
        "You are right"

    let toStringFunction (wasmMemory:Memory) implementation (key:int) (keyLen:int) (buffer:int) =
        let prompt = wasmMemory.ReadString(store, key, keyLen)
        let response = implementation prompt
        let bytestWritten = wasmMemory.WriteString(store, buffer, response)
        store.GC()
        bytestWritten

    let receiveString (wasmMemory:Memory) implementation (key:int) (keyLen:int)=
        let str =  wasmMemory.ReadString(store, key, keyLen)
        implementation str
        ()

    interface IDisposable with
        member this.Dispose()=
            store.Dispose()
            linker.Dispose()
            engine.Dispose()

    member this.Init()=
        linker.DefineWasi()
    
    member this.Execute(file:string)=
        use wasmModule = Module.FromFile(engine, file)

        let mutable wasmMemory:Memory = null;
        linker.Define("env", "log_external", Function.FromCallback(store, Action<int,int>(log |> receiveString wasmMemory)))
        linker.Define("env", "prompt_and_fill", Function.FromCallback(store, promptAndFill |> toStringFunction wasmMemory))
        let instance = linker.Instantiate(store, wasmModule)
        wasmMemory <- instance.GetMemory(store, "memory")

        let run = instance.GetAction(store, "_start")

        run.Invoke()