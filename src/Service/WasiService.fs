module WasiService

open System
open Wasmtime
    

type Runtime(moduleNamespace: string)=
    let engine = new Engine();
    let linker = new Linker(engine)
    let store = new Store(engine)
    let logs = System.Text.StringBuilder();
    let random = Random()
    do
        let config = WasiConfiguration()
        store.SetWasiConfiguration(config)

    let log (msg:string) = 
        logs.AppendLine(msg)
        |> ignore

    let logValue text n=
        n |> sprintf "%s %d" text |> log 
        n

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

    let startModule (file:string)=
        use wasmModule = Module.FromFile(engine, file)

        let instance = linker.Instantiate(store, wasmModule)

        let start = instance.GetAction(store, "_start")
        start.Invoke()
        instance

    interface IDisposable with
        member this.Dispose()=
            store.Dispose()
            linker.Dispose()
            engine.Dispose()

    member this.Init()=
        linker.DefineWasi()
        let getMemory (caller:Caller)=
            caller.GetMemory("memory")
        // Order pizza
        linker.Define("env", "log_external", Function.FromCallback(store, Action<Caller,int,int>(fun c k kl -> receiveString (getMemory c) log k kl )))
        linker.Define("env", "prompt_and_fill", Function.FromCallback(store, Func<Caller,int,int,int,int>(fun c k kl b -> toStringFunction (getMemory c) promptAndFill k kl b)))

        // sample
        linker.Define("env", "put_value", Function.FromCallback(store, Func<int>(random.Next >> logValue "Generated value:")))

    member this.Logs=
        logs.ToString();
    
    member this.ExecutePizzaExample(file:string)=
        let instance = startModule(file)
        let run = instance.GetAction<int>(store, "call_dialog")
        run.Invoke(random.Next())
        
    member this.ExecuteGetResultInt64(file:string)=
        let instance = startModule(file)
        let run = instance.GetFunction<int>(store, "get_result")

        run.Invoke()
        |> sprintf "Response from wasm: '%d'"
        |> log