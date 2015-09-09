﻿module Bullets

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open RxNA.Renderer
open Types
open Collisions
open GameInput

let initialBullets renderResources = List.Empty

/// Spawn new bullets if player is currently shooting
let private spawnBullets res (playerInput:GameAction []) (player:Mob) state =
    if Array.exists (fun x -> x = Attack) playerInput
       then List.append state [ { location = player.location;
                                  speed = { dx = 0.0f; dy = -750.0f };
                                  texture = res.textures.Item "laser" } ]
       else state

/// Render a single bullet
let private renderBullet res bullet =
    let texture = bullet.texture
    res.spriteBatch.Draw(texture, Vector2(bullet.location.x - (float32)texture.Width / 2.0f, bullet.location.y - (float32)texture.Height / 2.0f), Color.White)

/// Does given bullet intersect with any of the enemies?
let isHit bullet enemies =
    not (List.map (fun enemy -> collision bullet enemy) enemies
         |> List.filter Option.isSome
         |> List.isEmpty)

/// Pipeline to handle updating bullets state
let bulletsUpdater (res:RenderResources) state (playerInput, (enemies, (player, gameTime))) =
    spawnBullets res playerInput player state 
    |> List.map (fun bullet -> { bullet with location = bullet.location + bullet.speed * timeCoeff gameTime})
    |> List.filter (fun bullet -> bullet.location.y > 0.0f)
    |> List.filter (fun bullet -> not <| isHit bullet enemies)

/// Render given bullets state
let bulletsRenderer bullets res =
    Option.iter 
    <| List.iter (renderBullet res) 
    <| bullets
