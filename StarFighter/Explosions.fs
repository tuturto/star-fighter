module Explosions

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open GameInput
open RxNA.Renderer 

type ExplosionInput =
    { collisions: BulletCollisionInfo list option
      time: GameTime option }

let initialExplosions renderResources = List.Empty

let private spawnExplosions renderResources (state:Mob list) input =
    match input.collisions with
        | None -> state
        | Some s -> 
            if List.isEmpty s
               then state
               else
                    let time = (List.head s).Time
                    List.map (collisionToMob renderResources time) s
                    |> List.append state

let explosionUpdater renderResources (state:Mob list) input =
    if input.time.IsNone
       then 
            spawnExplosions renderResources state input
       else 
            let time = input.time.Value
            spawnExplosions renderResources state input
            |> List.filter (fun explosion -> not (isFinished explosion.texture time))
            |> List.map (fun explosion -> { explosion with location = explosion.location + explosion.speed * timeCoeff time })

let private renderExplosion res time explosion =
    match isFinished explosion.texture time with
        | true -> ()
        | false ->
            let texture = currentFrame time explosion.texture
            res.spriteBatch.Draw(texture,
                                 Vector2(explosion.location.x - (float32)texture.Width / 2.0f,
                                         explosion.location.y - (float32)texture.Height / 2.0f), Color.White)

let explosionRenderer explosions res time =
    Option.iter
    <| List.iter (renderExplosion res time)
    <| explosions

let mapCollisionsToExplosions collisions =
    { collisions = Some collisions;
      time = None; }

let mapGameTimeToExplosions gameTime =
    { collisions = None;
      time = Some gameTime; }
