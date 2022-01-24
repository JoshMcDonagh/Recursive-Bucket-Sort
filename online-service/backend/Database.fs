module Database

open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema
open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions
open System

[<CLIMutable>]
type ResultMetadata = {
    OperatingSystem: string
    Architecture: string
    Cpu: string
    LanguageVersion: string
    SortingAlgorithmImplementationVersion: string
    ArrayDistribution: string
    ArrayElementType: string
}

[<CLIMutable>]
type ApiKey = {
    [<Key>] Id: string // uuid
    IsAdmin: bool
}

[<CLIMutable>]
type ResultSet = {
    [<Key>] Id: int
    ApiKeyId: string
    ApiKey: ApiKey
}

[<CLIMutable>]
type Result = {
    [<Key>] Id: int
    Taken: DateTimeOffset
    Duration: TimeSpan
    Language: string
    SortingAlgorithm: string
    ArraySize: int
    
    ResultSetId: int
    ResultSet: ResultSet

    [<Column(TypeName = "jsonb")>] Metadata: ResultMetadata
}

type JoshyContext(Options) =  
    inherit DbContext(Options)
    
    [<DefaultValue>] val mutable resultSets : DbSet<ResultSet>
    member this.ResultSets with get() = this.resultSets and set v = this.resultSets <- v

    [<DefaultValue>] val mutable results : DbSet<Result>
    member this.Results with get() = this.results and set v = this.results <- v

    [<DefaultValue>] val mutable apiKeys : DbSet<ApiKey>
    member this.ApiKeys with get() = this.apiKeys and set v = this.apiKeys <- v

    override _.OnModelCreating builder =
        builder.RegisterOptionTypes() // enables option values for all entities