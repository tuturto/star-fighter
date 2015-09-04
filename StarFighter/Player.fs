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

let initialPlayer res = 
    { location = { x = 464.0f;
                   y = 600.0f; }
      dx = 0.0f;
      dy = 0.0f;
      texture = res.textures.Item "player" } 

let addMovementActions state action =
    match action with
        | Move (0.0f, 0.0f) -> { state with dx=0.0f; dy=0.0f; }
        | Move (dx, 0.0f) -> { state with dx=dx; }
        | Move (0.0f, dy) -> { state with dy=dy; }
        | Move (dx, dy) -> { state with dx=dx; dy=dy; }
        | _ -> state

let playerUpdater (state:Mob) ((actions:GameAction []), (time:GameTime)) =
    let speed = (float32)(time.ElapsedGameTime.TotalMilliseconds / 1000.0)
    let newState = Array.fold (fun acc item ->
                                    match item with
                                        | Move (dx, dy) -> addMovementActions acc item
                                        | Attack -> state)
                              state
                              actions
    let newLocation = { x=newState.location.x + newState.dx * speed * 250.0f;
                        y=newState.location.y + newState.dy * speed * 250.0f; }
    { newState with location = newLocation }

let playerRenderer state res = 
    Option.iter (fun player ->
                    res.spriteBatch.Draw(player.texture, Vector2(player.location.x - 48.0f, player.location.y - 48.0f), Color.White)) state
