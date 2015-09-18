module Bullets

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 
open ExtCore.Collections

open RxNA.Renderer
open Types
open Collisions
open GameInput

let initialBullets renderResources = List.Empty

/// Spawn new bullets if player is currently shooting
let private spawnBullets res gameTime (playerInput:GameAction []) (player:Mob) state =
    if Array.exists (fun x -> x = Attack) playerInput
       then List.append state [ { location = player.location;
                                  speed = { dx = 0.0f; dy = -750.0f };
                                  texture = convert gameTime <| res.textures.Item "laser";
                                  hp = 1 } ]
       else state

/// Render a single bullet
let private renderBullet res time bullet =
    let texture = currentFrame time bullet.texture 
    res.spriteBatch.Draw(texture, Vector2(bullet.location.x - (float32)texture.Width / 2.0f, bullet.location.y - (float32)texture.Height / 2.0f), Color.White)

/// Does given bullet intersect with any of the enemies?
let isHit gameTime bullet enemies =
    not (List.map (fun enemy -> collision gameTime bullet enemy) enemies
         |> List.filter Option.isSome
         |> List.isEmpty)

let checkCollisions gameTime enemies bullet =
    let collData = List.map (fun enemy -> 
                        let coll = collision gameTime bullet enemy
                        if coll.IsNone
                           then NoCollision (bullet, gameTime)
                           else EnemyCollision (enemy, bullet, coll.Value, gameTime)) enemies
                   |> List.filter (function
                                       | NoCollision _ -> false
                                       | EnemyCollision _ -> true)
    if List.isEmpty collData
       then NoCollision (bullet, gameTime)
       else List.last collData

/// Pipeline to handle updating bullets state
let bulletsUpdater (res:RenderResources) state (playerInput, (enemies, (player, gameTime))) =
    spawnBullets res gameTime playerInput player state 
    |> List.map (fun bullet -> { bullet with location = bullet.location + bullet.speed * timeCoeff gameTime})
    |> List.filter (fun bullet -> bullet.location.y > 0.0f)
    |> List.map (checkCollisions gameTime enemies)
    |> tap (fun x -> List.filter (function
                                      | NoCollision _ -> false
                                      | EnemyCollision _ -> true) x
                     |> enemyBulletCollisions.OnNext)
    |> List.filter (fun bullet -> not (bullet.Collided))
    |> List.map (fun bullet -> bullet.Bullet)

/// Render given bullets state
let bulletsRenderer bullets res time =
    Option.iter 
    <| List.iter (renderBullet res time) 
    <| bullets
