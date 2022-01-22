open Giraffe
open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open Giraffe.EndpointRouting
open Database
open ApiKeyService

let endpoints =
    [
        GET [
            route "/ping" (text "pong")
        ]
    ]
    @ Api.endpoints

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app
        .UseRouting()
        .UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
    |> ignore

let configureServices (services : IServiceCollection) =
    services
        .AddGiraffe()
        .AddSingleton<ApiKeyService>()
        .AddDbContext<JoshyContext>(
            fun options ->
                Environment.GetEnvironmentVariable("DB_CONN_STRING")
                |> options.UseNpgsql 
                |> ignore
        ) |> ignore

    // Apply any migrations
    services
        .BuildServiceProvider()
        |> fun services ->
            services.GetRequiredService<JoshyContext>().Database.Migrate()

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0