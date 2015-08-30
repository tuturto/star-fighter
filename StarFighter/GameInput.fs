module GameInput

open System.Reactive.Subjects
open FSharp.Control.Reactive

open Microsoft.Xna.Framework.Input
open RxNA.Input

open Types

type MenuAction =
    | ExitGame
    | StartGame

type GameAction =
    | Move of float * float
    | Attack

let menuTimeStream =
    gameTimeStream
    |> Observable.filter (fun x -> gameModeStream.Value = Menu)

let gameRunningTimeStream =
    gameTimeStream
    |> Observable.filter (fun x -> gameModeStream.Value = GameRunning)

let menuActionStreamKeys = keysPressedStream
                           |> Observable.filter
                                (fun x -> gameModeStream.Value = Menu)
                           |> Observable.map
                                (fun x -> x |> Array.map (function | Keys.Escape -> Some(ExitGame)
                                                                   | Keys.Space -> Some(StartGame)
                                                                   | _ -> None)
                                            |> Array.filter (fun item -> item.IsSome)
                                            |> Array.map (fun item -> item.Value))
                           |> Observable.filter
                                (fun x -> x |> Array.length > 0)

let menuActionStreamPad = gamePadStream 
                          |> Observable.filter
                                (fun x -> gameModeStream.Value = Menu)
                          |> Observable.map
                                (fun x -> [| (if x.IsButtonDown Buttons.B then Some(ExitGame) else None);
                                             (if x.IsButtonDown Buttons.A then Some(StartGame) else None) |]
                                          |> Array.filter (fun item -> item.IsSome)
                                          |> Array.map (fun item -> item.Value))
                          |> Observable.filter
                                (fun x -> x |> Array.length > 0)

let menuActionStream = menuActionStreamKeys
                       |> Observable.merge menuActionStreamPad 
