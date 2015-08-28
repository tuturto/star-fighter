module GameInput

open System.Reactive.Subjects
open FSharp.Control.Reactive

open Microsoft.Xna.Framework.Input
open RxNA.Input

type MenuAction =
    | ExitGame
    | StartGame

type GameAction =
    | Move of float * float
    | Attack

let menuActionStreamKeys = keysPressedStream
                           |> Observable.map
                                (fun x -> x |> Array.map (fun item -> if item = Keys.Escape then Some(ExitGame) else None)
                                            |> Array.filter (fun item -> item.IsSome)
                                            |> Array.map (fun item -> item.Value))
                           |> Observable.filter
                                (fun x -> x |> Array.length > 0)

let menuActionStreamPad = gamePadStream 
                          |> Observable.map
                                (fun x -> [| (if x.IsButtonDown Buttons.B then Some(ExitGame) else None) |]
                                          |> Array.filter (fun item -> item.IsSome)
                                          |> Array.map (fun item -> item.Value))
                          |> Observable.filter
                                (fun x -> x |> Array.length > 0)

let menuActionStream = menuActionStreamKeys
                       |> Observable.merge menuActionStreamPad 
