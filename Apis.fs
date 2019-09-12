namespace FunWebService

module Apis =
  open System.Net.Http
  open Microsoft.Azure.WebJobs
  open Microsoft.Azure.WebJobs.Extensions.Http
  open Microsoft.Extensions.Logging
  open Microsoft.AspNetCore.Mvc
  open Dtos
  open Storage
  open Microsoft.Azure.Cosmos.Table

  let toLower (str: string) = str.ToLower()

  // Post Person data api logic
  let personDtoToPersonRow (person: Person.Root) =
    let personRow = new PersonRow(person.Age, person.Hometown)

    personRow.FirstName <- person.FirstName
    personRow.LastName <- person.LastName
    personRow.HomeTown <- person.Hometown
    personRow.EyeColor <- person.EyeColor
    personRow.HeightInCentimeters <- person.HeightInCentimeters
    personRow.WeightInlbs <- person.WeightInlbs
    personRow.Occupation <- person.Occupation
    personRow.Race <- person.Race
    personRow.Gender <- person.Gender
    personRow.ShoeSize <- person.ShoeSize
    personRow.IsMarried <- person.IsMarried
    personRow.Age <- person.Age
    personRow.BirthDate <- person.BirthDate
    
    personRow

  let savePersonRowsToCloudTable (log: ILogger) personRows =
    getCloudTable log "people" >>= fun cloudTable ->
      writeBatchToTableStore log cloudTable personRows

  // Query API logic
  let opToQueryOp = function 
    | "eq" -> Some QueryComparisons.Equal
    | "gt" -> Some QueryComparisons.GreaterThan
    | "lt" -> Some QueryComparisons.LessThan
    | "neq" -> Some QueryComparisons.NotEqual
    | "gte" -> Some QueryComparisons.GreaterThanOrEqual
    | "lte" -> Some QueryComparisons.LessThanOrEqual
    | _ -> None

  let parseQueryRequest (query: QueryDto.Root) =
    query.Op 
      |> toLower
      |> opToQueryOp
      |> Option.map (fun op -> 
        TableQuery.GenerateFilterCondition(
          query.Column,
          op,
          query.Value
        )
      )

  let queryBy log filterCondition =
    getCloudTable log "people" >>= fun cloudTable -> 
      queryColumnBy log cloudTable filterCondition 

  [<FunctionName("PostPersonData")>]
  let PostPersonData([<HttpTrigger(AuthorizationLevel.Function, "post")>] req: HttpRequestMessage, log: ILogger) = 
    
    let cloudStorageResult =
      async {
        let! content = 
          req.Content.ReadAsStringAsync() |> Async.AwaitTask
      
        return content
          |> Person.Parse
          |> Array.map personDtoToPersonRow
          |> savePersonRowsToCloudTable log
      }
      |> Async.RunSynchronously

    match cloudStorageResult with
      | Successful _ -> StatusCodeResult 204
      | Failure -> StatusCodeResult 500
          
  [<FunctionName("QueryByColumn")>]
  let GetQueryByHometown([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query")>] req: HttpRequestMessage, log: ILogger) : IActionResult =
    
    let queryResults = 
      req.Content.ReadAsStringAsync() 
        |> Async.AwaitTask 
        |> Async.RunSynchronously
        |> QueryDto.Parse
        |> parseQueryRequest
        |> Option.map (queryBy log)
        |> Option.defaultValue Failure

    match queryResults with
      | Failure -> NotFoundResult() :> IActionResult
      | Successful results -> OkObjectResult(results) :> IActionResult
        
