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

let private spawnExplosions renderResources (state:Mob list) (deadEnemies:Enemy list) collisions time =
    List.map (impactExplosions renderResources time) collisions
    |> List.append <| List.map (enemyExplosions renderResources time) deadEnemies
    |> List.append state

let explosionUpdater renderResources (state:Mob list) (deadEnemies, (collisions, time)) =
    spawnExplosions renderResources state deadEnemies collisions time
    |> List.filter (fun explosion -> not (isFinished explosion.texture time))
    |> List.map (fun explosion -> { explosion with location = explosion.location + explosion.speed * timeCoeff time })

let private renderExplosion res time (explosion:Mob) =
    match isFinished explosion.texture time with
        | true -> ()
        | false ->
            let texture = currentFrame time explosion.texture
            res.spriteBatch.Draw(texture,
                                 Vector2(explosion.location.x - (float32)texture.Width / 2.0f,
                                         explosion.location.y - (float32)texture.Height / 2.0f), Color.White)

let explosionRenderer (explosions:Mob list option) res time =
    Option.iter
    <| List.iter (renderExplosion res time)
    <| explosions

let mapCollisionsToExplosions collisions =
    { collisions = Some collisions;
      time = None; }

let mapGameTimeToExplosions gameTime =
    { collisions = None;
      time = Some gameTime; }
