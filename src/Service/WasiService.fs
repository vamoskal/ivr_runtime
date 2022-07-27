module WasiService

open System
open Wasmtime
    

type Runtime(moduleNamespace: string)=
    let engine = new Engine();
    let linker = new Linker(engine)
    let store = new Store(engine)
    let logs = System.Text.StringBuilder();
    do
        let config = WasiConfiguration()
        store.SetWasiConfiguration(config)

    let log (msg:string) = 
        logs.AppendLine(msg)
        |> ignore

    let promptAndFill(prompt:string)=
        logs.AppendLine(prompt) |> ignore
        match prompt with
        | "Hello" -> "Hello to you too"
        | x when x.Contains("order") -> "pizza"
        | x when x.Contains("kind of pizza") -> "cheese"
        | _ -> "Unknown"


    let toStringFunction (wasmMemory:Memory) implementation (key:int) (keyLen:int) (buffer:int) =
        let prompt = wasmMemory.ReadString(store, key, keyLen)
        let response = implementation prompt
        let bytestWritten = wasmMemory.WriteString(store, buffer, response)
        bytestWritten

    let receiveString (wasmMemory:Memory) implementation (key:int) (keyLen:int)=
        let str =  wasmMemory.ReadString(store, key, keyLen)
        implementation str

    interface IDisposable with
        member this.Dispose()=
            store.Dispose()
            linker.Dispose()
            engine.Dispose()

    member this.Init()=
        linker.DefineWasi()

    member this.Logs=
        logs.ToString();
    
    member this.Execute(file:string)=
        use wasmModule = Module.FromFile(engine, file)

        let mutable wasmMemory:Memory = null;
        linker.Define("env", "log_external", Function.FromCallback(store, Action<int,int>(log |> receiveString wasmMemory)))
        linker.Define("env", "prompt_and_fill", Function.FromCallback(store, promptAndFill |> toStringFunction wasmMemory))
        let instance = linker.Instantiate(store, wasmModule)
        wasmMemory <- instance.GetMemory(store, "memory")

        let start = instance.GetAction(store, "_start")
        start.Invoke()
        
        let run = instance.GetAction<int>(store, "call_dialog")
        run.Invoke(Guid.NewGuid().GetHashCode())