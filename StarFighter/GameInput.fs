﻿module GameInput

open System.Reactive.Subjects
open FSharp.Control.Reactive

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open RxNA.Input

open Types

type MenuAction =
    | ExitGame
    | StartGame

type GameAction =
    | Move of float32 * float32
    | Attack

let menuTimeStream =
    gameTimeStream
    |> Observable.filter (fun x -> gameModeStream.Value = Menu)
    |> Observable.publish

let gameRunningTimeStream =
    gameTimeStream
    |> Observable.filter (fun x -> gameModeStream.Value = GameRunning)
    |> Observable.publish

let readyScreenTimeStream =
    gameTimeStream
    |> Observable.filter (fun x -> gameModeStream.Value = ReadyScreen)
    |> Observable.publish

let timeCoeff (gameTime:GameTime) = 
    (float32)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0)
