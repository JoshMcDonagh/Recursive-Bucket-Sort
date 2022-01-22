module Api

open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Database
open System
open Thoth.Json.Net
open ApiKeyService
open System.IO

[<CLIMutable>]
type PutSubModel = {
    Taken: DateTimeOffset
    Duration: string
    Language: string
    SortingAlgorithm: string
    ArraySize: int
    Metadata: ResultMetadata
}

[<CLIMutable>]
type PutModel = {
    ApiKey: string
    Recordings: PutSubModel[]
}

[<CLIMutable>]
type GetSetsModel = {
    Page: int
}

[<CLIMutable>]
type GetResultsModel = {
    ResultSetId: int
}

[<CLIMutable>]
type PostTokenModel = {
    ApiKey: string
    IsAdmin: bool
}

// If you've never seen this language before, behold, active patterns.
// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/active-patterns
//
// btw this language is *crazy* with some of the stuff it can express and do.
let (|Seconds|NotANumber|) (str : string) =
    let mutable value = 0.0
    match Double.TryParse(str, &value) with
    | true -> Seconds value
    | false -> NotANumber

let known_archs         = ["x86"; "x86_64"; "arm64"]
let known_languages     = ["python"]
let known_algorithms    = ["TimSort"; "QuickSort"; "RecursiveBucketSort"]
let known_os            = ["Windows"; "Linux"; "Mac"]
let known_distributions = ["FullyRandom"; "PartiallyRandom"]

let (|ValidRequest|InvalidRequest|) (model : PutSubModel, rs : ResultSet) =
    if model.Taken > DateTimeOffset.UtcNow then
        InvalidRequest "This result was taken in the future?"
    elif not (List.contains model.Metadata.Architecture known_archs) then
        InvalidRequest "Architecture not known. Open an issue if this is a mistake."
    elif not (List.contains model.Language known_languages) then
        InvalidRequest "Language not known. Open an issue if this is a mistake."
    elif not (List.contains model.SortingAlgorithm known_algorithms) then
        InvalidRequest "Sorting Algorithm not known. Open an issue if this is a mistake."
    elif model.ArraySize <= 0 then
        InvalidRequest "ArraySize is <= 0, this is not valid."
    elif not (List.contains model.Metadata.OperatingSystem known_os) then
        InvalidRequest "Operating System not known. Open an issue if this is a mistake."
    elif not (List.contains model.Metadata.ArrayDistribution known_distributions) then
        InvalidRequest "Array Distribution not known. Open an issue if this is a mistake."
    else
        match model.Duration with
        | Seconds s -> ValidRequest {
                Id = 0
                Taken = DateTimeOffset model.Taken.UtcDateTime
                Duration = s |> TimeSpan.FromSeconds
                Language = model.Language
                SortingAlgorithm = model.SortingAlgorithm
                ArraySize = model.ArraySize
                Metadata = model.Metadata
                ResultSet = rs
                ResultSetId = rs.Id
            }
        | NotANumber -> InvalidRequest "Duration is not a valid number"

let put =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.ReadBodyBufferedFromRequestAsync()

            return! match Decode.Auto.fromString<PutModel>(body) with
                    | Error msg -> RequestErrors.unprocessableEntity (text msg) next ctx
                    | Ok m ->
                        let db = ctx.RequestServices.GetRequiredService<JoshyContext>()
                        let apiKeyService = ctx.RequestServices.GetRequiredService<ApiKeyService>()

                        if not (m.ApiKey |> apiKeyService.KeyExists) then
                            RequestErrors.badRequest (text "API Key does not exist") next ctx
                        elif (m.ApiKey |> apiKeyService.IsRateLimited) then
                            RequestErrors.tooManyRequests (text "You've hit your rate limit, try again in 1 minute.") next ctx
                        elif m.Recordings.Length = 0 then
                            RequestErrors.badRequest (text "No recordings were submitted") next ctx
                        else
                            let apiKey = db.ApiKeys.Find(m.ApiKey)
                            let rs = {
                                Id = 0
                                ApiKeyId = apiKey.Id
                                ApiKey = apiKey
                            }
                            rs |> db.Add |> ignore
                            let rec handleModel (model: PutSubModel) (array: PutSubModel[]) =
                                match (model, rs) with
                                | InvalidRequest error -> Some error
                                | ValidRequest req ->
                                    req |> db.Add |> ignore
                                    if array.Length = 0 then
                                        None
                                    else
                                        handleModel array.[0] array.[1..]

                            match handleModel m.Recordings.[0] m.Recordings.[1..] with
                            | Some error -> RequestErrors.unprocessableEntity (text error) next ctx
                            | None -> 
                                db.SaveChanges() |> ignore
                                Successful.OK "Success" next ctx
        }

let getSets =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.ReadBodyBufferedFromRequestAsync()

            return! match Decode.Auto.fromString<GetSetsModel>(body) with
                    | Error msg -> RequestErrors.unprocessableEntity (text msg) next ctx
                    | Ok model ->
                        let db = ctx.RequestServices.GetRequiredService<JoshyContext>()
                        
                        Successful.OK (query {
                            for result in db.ResultSets do
                            sortByDescending result.Id
                            select {|
                                Set = result
                                Taken = query {
                                    for res in db.Results do
                                    where (res.ResultSetId = result.Id)
                                    select res.Taken
                                    exactlyOne
                                }
                            |}
                            skip (model.Page * 100)
                            take 100
                        }) next ctx
        }

let get =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.ReadBodyBufferedFromRequestAsync()

            return! match Decode.Auto.fromString<GetResultsModel>(body) with
                    | Error msg -> RequestErrors.unprocessableEntity (text msg) next ctx
                    | Ok model ->
                        let db = ctx.RequestServices.GetRequiredService<JoshyContext>()
                        
                        Successful.OK (query {
                            for result in db.Results do
                            where (result.ResultSetId = model.ResultSetId)
                            select result
                        }) next ctx
        }

let postToken =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! body = ctx.ReadBodyBufferedFromRequestAsync()

            return! match Decode.Auto.fromString<PostTokenModel>(body) with
                    | Error msg -> RequestErrors.unprocessableEntity (text msg) next ctx
                    | Ok m ->
                        let apiKeyService = ctx.RequestServices.GetRequiredService<ApiKeyService>()
                        let db = ctx.RequestServices.GetRequiredService<JoshyContext>()

                        if (query { for key in db.ApiKeys do count }) = 0 then
                            let key = apiKeyService.GenerateKey(true)
                            Successful.ok (json {| ApiKey = key.Id |}) next ctx
                        elif not (m.ApiKey |> apiKeyService.KeyExists) then
                            RequestErrors.unprocessableEntity (text "Api Key does not exist") next ctx
                        elif not (m.ApiKey |> apiKeyService.IsAdminKey) then
                            RequestErrors.unprocessableEntity (text "Api Key is not an admin key") next ctx
                        elif (m.ApiKey |> apiKeyService.IsRateLimited) then
                            RequestErrors.tooManyRequests (text "You're rate limited, try again in 1 minute.") next ctx
                        else
                            let key = apiKeyService.GenerateKey(m.IsAdmin)
                            Successful.ok (json {| ApiKey = key.Id |}) next ctx
        }

let uploadCsv =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            return!
                match ctx.Request.HasFormContentType with
                | false -> redirectTo false "/upload?error=not_a_form" next ctx
                | true ->
                    if ctx.Request.Form.Files.Count <> 1 then
                            redirectTo false "/upload?error=file_count_mismatch" next ctx
                    elif ctx.Request.Form.Files.[0].Length >= int64 (1024 * 1000) then
                            redirectTo false "/upload?error=file_too_large" next ctx
                    else
                        let db = ctx.RequestServices.GetRequiredService<JoshyContext>()
                        let apiKeyService = ctx.RequestServices.GetRequiredService<ApiKeyService>()
                        use stream = new StreamReader(ctx.Request.Form.Files.[0].OpenReadStream())
                        let apiKeyRaw = ctx.GetFormValue("api-key").Value
                        let apiKey = db.ApiKeys.Find(apiKeyRaw)
                        let rs = {
                            Id = 0
                            ApiKeyId = apiKeyRaw
                            ApiKey = apiKey
                        }
                        rs |> db.Add |> ignore
                        let records = (
                            seq {
                                while not stream.EndOfStream do
                                    stream.ReadLine()
                            } 
                            |> Seq.filter (fun line -> not (line |> String.IsNullOrWhiteSpace))
                            |> Seq.map (fun line -> line.Split(','))
                            |> Seq.toArray
                        )

                        if records.Length = 0 then
                            redirectTo false "/upload?error=no_records" next ctx
                        elif not (apiKeyRaw |> apiKeyService.KeyExists) then
                            redirectTo false "/upload?error=api_key_not_exists" next ctx
                        elif (apiKeyRaw |> apiKeyService.IsRateLimited) then
                            redirectTo false "/upload?error=rate_limited" next ctx
                        else
                            let rec doProcess (record: string[]) (records: string[][]) =
                                {
                                    Id = 0
                                    Taken = record.[0] |> DateTimeOffset.Parse
                                    Duration = record.[1] |> float |> TimeSpan.FromSeconds
                                    Language = record.[2]
                                    SortingAlgorithm = record.[3]
                                    ArraySize = int record.[4]
                                    ResultSetId = rs.Id
                                    ResultSet = rs
                                    Metadata = {
                                        OperatingSystem = record.[5]
                                        Architecture = record.[6]
                                        Cpu = record.[7]
                                        LanguageVersion = record.[8]
                                        SortingAlgorithmImplementationVersion = record.[9]
                                        ArrayDistribution = record.[10]
                                        ArrayElementType = record.[11]
                                    }
                                } |> db.Add |> ignore
                                if records.Length = 0 then
                                    db.SaveChanges() |> ignore
                                    redirectTo false "/" next ctx
                                else
                                    doProcess records.[0] records.[1..]

                            doProcess records.[0] records.[1..]
        }

let endpoints = [
    PUT [
        route "/results" put
    ]

    POST [
        route "/token" postToken
        route "/upload" uploadCsv

        // These are POSTs because js won't let me use requests bodies with GET
        route "/resultSets" getSets
        route "/results" get
    ]
]