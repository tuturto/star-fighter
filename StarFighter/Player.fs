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
    NormalPlayer ({ location = { x = 464.0f
                                     y = 600.0f }
                    speed = { dx = 0.0f
                              dy = 0.0f }
                    lives = 3
                    texture = convert time <| res.textures.Item "player" } )

let addMovementActions (ship:NormalPlayerInfo) action =
    let newSpeed = match action with
                       | Move (0.0f, 0.0f) -> { dx=0.0f; dy=0.0f; }
                       | Move (dx, 0.0f) -> { ship.speed with dx=dx; }
                       | Move (0.0f, dy) -> { ship.speed with dy=dy; }
                       | Move (dx, dy) -> { dx=dx; dy=dy; }
                       | _ -> ship.speed
    { ship with speed = newSpeed }

let playerUpdater playSound (state:Player) ((enemies:Enemy list), ((actions:GameAction []), (time:GameTime))) =
    match state with
        | NormalPlayer (ship:NormalPlayerInfo) ->
                let collisionPoints = List.map (collision time ship) enemies
                                      |> List.filter (fun x -> x.IsSome)
                let newState = Array.fold (fun acc item ->
                                               match item with
                                                   | Move (dx, dy) -> addMovementActions acc item
                                                   | Attack -> ship)
                                           ship
                                           actions
                if List.isEmpty collisionPoints
                   then NormalPlayer ({ newState with location = newState.location + newState.speed * timeCoeff time * 250.0f })
                   else playSound Explosion
                        ExplodingPlayer ({ location = newState.location + newState.speed * timeCoeff time * 250.0f
                                           speed = newState.speed
                                           explosionTime = time.TotalGameTime.TotalMilliseconds
                                           lives =  ship.lives })
        | ExplodingPlayer _ -> state

let playerRenderer state res time = 
    Option.iter (fun player ->
                    match player with
                        | NormalPlayer ship ->
                                let texture = currentFrame time ship.texture 
                                res.spriteBatch.Draw(texture, 
                                                     Vector2(ship.location.x - (float32)texture.Width / 2.0f, 
                                                             ship.location.y - (float32)texture.Height / 2.0f), 
                                                     Color.White)
                        | ExplodingPlayer _ -> ()) state
