module Player

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open RxNA.Renderer 
open RxNA.Input 
open GameInput
open Collisions

let playerActionStreamKeys = keysPressedStream
                             |> Observable.filter
                                  (fun x -> gameModeStream.Value = GameRunning)
                             |> Observable.map
                                  (fun x -> x |> Array.map (function | Keys.Left -> Some(Move (-1.0f, 0.0f))
                                                                     | Keys.Right -> Some(Move (1.0f, 0.0f))
                                                                     | Keys.Up -> Some(Move (0.0f, -1.0f))
                                                                     | Keys.Down -> Some(Move (0.0f, 1.0f))
                                                                     | Keys.Space -> Some(Attack)
                                                                     | _ -> None)
                                              |> Array.filter (fun item -> item.IsSome)
                                              |> Array.map (fun item -> item.Value))

let playerActionStreamPad = gamePadStream 
                            |> Observable.filter
                                  (fun x -> gameModeStream.Value = GameRunning)
                            |> Observable.map
                                  (fun x -> [| (if x.IsButtonDown Buttons.A then Some(Attack) else None);
                                               Some(Move (x.ThumbSticks.Left.X, -x.ThumbSticks.Left.Y)) |]
                                            |> Array.filter (fun item -> item.IsSome)
                                            |> Array.map (fun item -> item.Value))

let playerActionStream = playerActionStreamKeys 
                         |> Observable.zip playerActionStreamPad 
                         |> Observable.map (fun (a, b) -> Array.concat [a; b])
                         |> Observable.publish

let initialPlayer res time = 
    { location = { x = 464.0f;
                   y = 600.0f; }
      speed = { dx = 0.0f;
                dy = 0.0f; }
      texture = convert time <| res.textures.Item "player";
      hp = 1; } 

let addMovementActions state action =
    let newSpeed = match action with
                       | Move (0.0f, 0.0f) -> { dx=0.0f; dy=0.0f; }
                       | Move (dx, 0.0f) -> { state.speed with dx=dx; }
                       | Move (0.0f, dy) -> { state.speed with dy=dy; }
                       | Move (dx, dy) -> { dx=dx; dy=dy; }
                       | _ -> state.speed
    { state with speed = newSpeed }

let playerUpdater (state:Mob) (enemies, ((actions:GameAction []), (time:GameTime))) =
    let collisionPoints = List.map (collision time state) enemies
                          |> List.filter (fun x -> x.IsSome)
    let newState = Array.fold (fun acc item ->
                                    match item with
                                        | Move (dx, dy) -> addMovementActions acc item
                                        | Attack -> state)
                              state
                              actions
    { newState with location = if List.isEmpty collisionPoints
                                  then newState.location + newState.speed * timeCoeff time * 250.0f
                                  else { x = 464.0f; y = 600.0f }}

let playerRenderer state res time = 
    Option.iter (fun player ->
                    let texture = currentFrame time player.texture 
                    res.spriteBatch.Draw(texture, 
                                         Vector2(player.location.x - (float32)texture.Width / 2.0f, 
                                                 player.location.y - (float32)texture.Height / 2.0f), 
                                         Color.White)) state
