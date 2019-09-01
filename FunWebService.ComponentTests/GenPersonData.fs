namespace FunWebServiceComponentTests

module GenPersonData =
  open System
  open FsCheck
  open FSharp.Data

  let private (<!>) (f: 'a -> 'b) (gen: 'a Gen) = gen.Select(fun v -> f v)
  let private (<!!>) g f = f <!> g
  let private (<*>) (f: Gen<'a -> 'b>) (v: 'a Gen) =
    f.SelectMany(fun f' -> v.Select(fun v' -> f' v'))

  let private (>>=) (gen: 'a Gen) (f: 'a -> Gen<'b>) = gen.SelectMany(fun v -> f v)

  let private ``"x"`` = JsonValue.String
  let inline private ``+x`` v = v |> decimal |> JsonValue.Number
  let private ``true|false`` = JsonValue.Boolean
  let private ``time -> "x"`` (x: DateTime) = x.ToString("o") |> ``"x"``
  let private ``[...]`` = Array.ofSeq >> JsonValue.Array

  let private genStringWithMaxLengthOf n =
    Gen.choose (1, n) >>= fun max -> 
    Gen.listOfLength max Arb.generate<char> <!!> 
    String.Concat

  let private genGender =
    Gen.elements ["male"; "female"; "other"]

  let private genHometown =
    Gen.elements ["Boston"; "New York"; "Pittsburgh"; "LA"; "Miami"]

  let private genInRangeInclusive lower upper =
    Gen.choose (lower, upper)

  let makePersonJson firstName lastName hometown eyeColor height weight occupation race gender
    shoeSize isMarried age birthDate =
      
      JsonValue.Record [|
        "firstName", ``"x"`` firstName
        "lastName", ``"x"`` lastName
        "hometown", ``"x"`` hometown
        "eyeColor", ``"x"`` eyeColor
        "heightInCentimeters", ``+x`` height
        "weightInlbs", ``+x`` weight
        "occupation", ``"x"`` occupation
        "race", ``"x"`` race
        "gender", ``"x"`` gender
        "shoeSize", ``+x`` shoeSize
        "isMarried", ``true|false`` isMarried
        "age", ``+x`` age
        "birthDate", ``time -> "x"`` birthDate
      |]

  type PersonData() =
    static member ValidParams : Arbitrary<JsonValue> =
      let json =
        makePersonJson
          <!> genStringWithMaxLengthOf 20
          <*> genStringWithMaxLengthOf 20
          <*> genHometown
          <*> genStringWithMaxLengthOf 10
          <*> Arb.generate<int>
          <*> Arb.generate<int>
          <*> Arb.generate<string>
          <*> genStringWithMaxLengthOf 15
          <*> genGender
          <*> genInRangeInclusive 3 22
          <*> Arb.generate<bool>
          <*> genInRangeInclusive 0 120
          <*> Arb.generate<DateTime>

      json |> Gen.listOf <!!> ``[...]`` |> Arb.fromGen