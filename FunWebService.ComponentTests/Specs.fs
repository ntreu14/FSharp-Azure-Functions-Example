namespace FunWebServiceComponentTests

module Specs =
  open Expecto
  open FSharp.Data
  open GenPersonData
  
  let ``given`` = id
  let ``when`` = id
  let ``then`` = id

  let ``posting person data to the fun web service`` host route (json: JsonValue) =
    Http.AsyncRequest(
      sprintf "%s/api/%s" host route,
      body = TextRequest (string json)
    )
    |> Async.RunSynchronously

  let ``should return an accepted status code`` (response: HttpResponse) =
    Expect.equal response.StatusCode 204 "the status code should be 204"

  let testCofig = 
    { 
      Expecto.FsCheckConfig.defaultConfig with 
        arbitrary=
          (typeof<PersonData>)::
          Expecto.FsCheckConfig.defaultConfig.arbitrary
    }

  let tests host =
    testList "fun web service specs"
      [
        testPropertyWithConfig { testCofig with maxTest = 10 } "valid calls don't yield errors" <| fun (people: JsonValue) ->
          
          ``given`` people
            |> ``then``
            |> ``posting person data to the fun web service`` host "PostPersonData"
            |> ``should return an accepted status code``
      ]