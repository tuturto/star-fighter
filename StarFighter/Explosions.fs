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

let private impactExplosions res time (info:BulletCollisionInfo) =
    let enemy = info.Enemy.Value 
    { Mob.location = info.Location.Value
      speed = enemy.speed
      texture = convert time <| res.textures.Item "small explosion" }

let private enemyExplosions res time (enemy:Enemy) =
    { Mob.location = enemy.location
      speed = enemy.speed
      texture = convert time <| res.textures.Item "large explosion" }

let private playerExplosions res (rng:System.Random) (time:GameTime) player =
    match player with
        | NormalPlayer _ -> List.empty
        | ExplodingPlayer ship -> 
                if ship.explosionTime + 100.0 > time.TotalGameTime.TotalMilliseconds 
                   then [ { Mob.location = { x = ship.location.x + (float32)(rng.NextDouble() * 50.0 - 25.0)
                                             y = ship.location.y + (float32)(rng.NextDouble() * 50.0 - 25.0) }
                            speed = ship.speed
                            texture = convert time <| res.textures.Item "large explosion" } ]
                   else List.empty

let private spawnExplosions renderResources rng playSound state player deadEnemies collisions time =
    if List.length deadEnemies > 0
       then playSound Explosion
       else ()
    List.map (impactExplosions renderResources time) collisions
    |> List.append <| List.map (enemyExplosions renderResources time) deadEnemies
    |> List.append <| playerExplosions renderResources rng time player
    |> List.append state

let explosionUpdater renderResources rng playSound (state:Mob list) (player, (deadEnemies, (collisions, time))) =
    spawnExplosions renderResources rng playSound state player deadEnemies collisions time
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
