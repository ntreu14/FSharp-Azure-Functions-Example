namespace FunWebService

module Storage =
  open System
  open Dtos
  open Microsoft.Azure.Cosmos.Table
  open Microsoft.Extensions.Logging

  type 'a StorageTableResult = Successful of 'a | Failure 

  let (>>=) storageTableResult f =
    match storageTableResult with
      | Successful a -> f a 
      | Failure -> Failure

  let getStorageConnectionString () =
    Environment.GetEnvironmentVariable ("storage_connectionString", EnvironmentVariableTarget.Process)

  let getCloudTable (log: ILogger) tableName =
    try 
      let cloudStorageAccount = 
        let connectionString = 
          getStorageConnectionString ()
        
        sprintf "connection string: %s" connectionString |> log.LogInformation
        connectionString |> CloudStorageAccount.Parse
    
      let tableClient = cloudStorageAccount.CreateCloudTableClient ()
      let table = tableClient.GetTableReference tableName
    
      table.CreateIfNotExistsAsync () 
        |> Async.AwaitTask 
        |> Async.Ignore
        |> ignore

      Successful table

    with ex ->
      sprintf "Failed getting a cloud table because %s" ex.Message |> log.LogError
      Failure

  let writeBatchToTableStore (log: ILogger) (table: CloudTable) (rows: PersonRow []) =
    try
      let batchOperations =
        rows 
          |> Array.groupBy (fun row -> (row.RowKey, row.PartitionKey))
          |> Array.map (fun (_, rowsByPartitionKey) -> 
            let batchOperation = TableBatchOperation()
            rowsByPartitionKey |> Array.iter batchOperation.InsertOrReplace
            batchOperation
          )

      let executeBatchAsync (t: CloudTable) batch =
        t.ExecuteBatchAsync batch
          |> Async.AwaitTask
          |> Async.RunSynchronously
          |> ignore
    
      batchOperations |> Array.Parallel.iter (executeBatchAsync table)

      Successful ()

    with ex -> 
      sprintf "Failed writing a batch because %s" ex.Message |> log.LogError
      Failure


  let queryColumnBy (log: ILogger) (table: CloudTable) filterCondition =
    try 
      let query = 
        TableQuery<PersonDto>().Where filterCondition

      query 
        |> table.ExecuteQuery<PersonDto>
        |> Successful
    
    with ex ->
      sprintf "Failed to query because %s" ex.Message |> log.LogError 
      Failure
