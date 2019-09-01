namespace FunWebService

module Dtos =
  open System
  open FSharp.Data
  open Microsoft.Azure.Cosmos.Table

  // Post Dtos
  type Person = JsonProvider<"""
  [
    {
      "firstName": "some first name",
      "lastName": "some last name",
      "hometown": "Pittsburgh",
      "eyeColor": "brown",
      "heightInCentimeters": 1234567,
      "weightInlbs": 1234567,
      "occupation": "some occupation",
      "race": "some race",
      "gender": "some gender",
      "shoeSize": "1234567",
      "isMarried": false,
      "age": 1234567,
      "birthDate": "1/1/2020"
    }
  ]""">

  type PersonRow (guid: int, hometown: string) =
    inherit TableEntity(partitionKey = hometown, rowKey = (string <| Guid.NewGuid()))
   
    member val FirstName = "" with get, set
    member val LastName = "" with get, set
    member val HomeTown = "" with get, set
    member val EyeColor = "" with get, set
    member val HeightInCentimeters = -1 with get, set
    member val WeightInlbs = -1 with get, set
    member val Occupation = "" with get, set
    member val Race = "" with get, set
    member val Gender = "" with get, set
    member val ShoeSize = -1 with get, set
    member val IsMarried = false with get, set
    member val Age = -1 with get, set
    member val BirthDate = DateTime.MinValue with get, set

  // Query Dtos
  type QueryDto = JsonProvider<"""
    {
      "column": "value",
      "op": "some op",
      "value": "value"
    }
  """>

  type PersonDto () =
    inherit TableEntity()

    member val FirstName = "" with get, set
    member val LastName = "" with get, set
    member val HomeTown = "" with get, set
    member val EyeColor = "" with get, set
    member val HeightInCentimeters = -1 with get, set
    member val WeightInlbs = -1 with get, set
    member val Occupation = "" with get, set
    member val Race = "" with get, set
    member val Gender = "" with get, set
    member val ShoeSize = -1 with get, set
    member val IsMarried = false with get, set
    member val Age = -1 with get, set
    member val BirthDate = DateTime.MinValue with get, set
