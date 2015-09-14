module Explosions

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open RxNA.Renderer 

type ExplosionInput =
    { collisions: BulletCollisionInfo list option
      time: GameTime option }

let initialExplosions renderResources = List.Empty

let explosionUpdater renderResources (state:Mob list) input =
    // add new explosions: BulletCollisionInfo list -> Mob list
    match input.collisions with
        | None -> state
        | Some s -> 
            if List.isEmpty s
               then state
               else
                    let time = (List.head s).Time
                    List.map (collisionToMob renderResources time) s
                    |> List.append state
    // remove expired explosions
    // move explosion mobs according to their speed
    // remove explosions that are out of bounds

let private renderExplosion res time explosion =
    match isFinished explosion.texture time with
        | true -> ()
        | false ->
            res.spriteBatch.Draw(currentFrame time explosion.texture,
                                 Vector2(explosion.location.x, explosion.location.y), Color.White)

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
