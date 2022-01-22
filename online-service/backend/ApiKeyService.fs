module ApiKeyService

open Microsoft.Extensions.DependencyInjection
open Database
open System

type RateLimitInfo = {
    Hits: int
    ResetAt: DateTime
}

type ApiKeyService(ScopeFactory : IServiceScopeFactory) =
    member val sfact = ScopeFactory
    member val rateLimitInfo = new Collections.Generic.Dictionary<string, RateLimitInfo>()

    member this.GenerateKey(isAdmin: bool) =
        use scope = this.sfact.CreateScope()
        let db = scope.ServiceProvider.GetRequiredService<JoshyContext>()
        let key = {
            Id = Guid.NewGuid().ToString()
            IsAdmin = isAdmin
        }
        key |> db.Add |> ignore
        db.SaveChanges() |> ignore
        key

    member this.KeyExists(key:string) =
        use scope = this.sfact.CreateScope()
        let db = scope.ServiceProvider.GetRequiredService<JoshyContext>()
        query {
            for k in db.ApiKeys do
            exists (k.Id = key)
        }

    member this.IsAdminKey(key:string) =
        use scope = this.sfact.CreateScope()
        let db = scope.ServiceProvider.GetRequiredService<JoshyContext>()
        query {
            for k in db.ApiKeys do
            where (k.Id = key)
            select k.IsAdmin
            exactlyOne
        }

    member this.IsRateLimited(key:string) =
        if not (this.KeyExists key) then
            true
        else
            if key |> this.rateLimitInfo.ContainsKey then
                if this.rateLimitInfo.[key].ResetAt <= DateTime.Now then
                    this.rateLimitInfo.[key] <- { 
                        this.rateLimitInfo.[key] with
                            Hits = 0
                            ResetAt = DateTime.Now.AddMinutes(1)
                    }
                let currHits = this.rateLimitInfo.[key].Hits
                this.rateLimitInfo.[key] <- {
                    this.rateLimitInfo.[key] with
                        Hits = currHits + 1
                }
                this.rateLimitInfo.[key].Hits >= 10
            else
                this.rateLimitInfo.Add(key, {
                    Hits = 1
                    ResetAt = DateTime.Now.AddMinutes(1)
                })
                false