namespace FunWebServiceComponentTests

module Main =
  open Expecto
  open Specs
  
  type Args = {
    Host: string
  }

  let (|EmptyText|NonEmptyText|) input =
    if System.String.IsNullOrWhiteSpace input
    then EmptyText
    else NonEmptyText input

  let toLower (str: string) = str.ToLower()

  let parse (argv: string []) =
    let pairs =
      argv
        |> Array.map (fun v -> 
          [|
            (v.Split [|'='|]).[0];
            System.String.Join("=", v.Split [|'='|] |> Array.tail)
          |]
        )
        |> Array.collect (
          function
            | [|NonEmptyText key; NonEmptyText value|] -> [|key, value|]
            | _ -> [||]
        )

    let (??-) key aDefault =
      pairs
        |> Seq.tryFind ((=) (toLower key) << toLower << fst)
        |> function Some v -> snd v | None -> aDefault

    {
      Host = "host" ??- "http://localhost:7071"
    }

  [<EntryPoint>]
  let main argv =
    let args = parse argv

    tests args.Host |> runTests { defaultConfig with ``parallel`` = false }