module Service.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Http
open System.Net.Http 

// ---------------------------------
// Web app
// ---------------------------------
let executePizzaExample  =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task{
            use runtime =new WasiService.Runtime("")
            runtime.Init()
            runtime.ExecutePizzaExample(Generator.currentDir + "\WebRoot\Client.wasm")
            return! setBodyFromString (runtime.Logs) next ctx
        }

let execute id=
    use runtime = new WasiService.Runtime("")
    runtime.Init()
    let success,exec = Generator.tryFindById id
    match success with
    | true ->
        runtime.ExecuteGetResultInt64(exec)
        text (runtime.Logs)
    | false -> setStatusCode 404 >=> text "Not Found"
    
let compile id=
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! codeSnippet = ctx.ReadBodyFromRequestAsync()
            let! wasmPath = Generator.generateWasm id codeSnippet
            return! Successful.ACCEPTED "" next ctx
        }

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> text "Hello World"
                route "/wasmi/pizza" >=> executePizzaExample
            ]
        POST >=>
            choose[
                routef "/wasmi/%i:execute" execute
                routef "/wasmi/%i:provision" compile
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001")
       .AllowAnyMethod()
       .AllowAnyHeader()
       |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0