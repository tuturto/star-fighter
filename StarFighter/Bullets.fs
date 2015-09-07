module Bullets

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open RxNA.Renderer
open Types
open GameInput

let initialBullets renderResources = List.Empty

/// Spawn new bullets if player is currently shooting
let private spawnBullets (playerInput:GameAction []) (player:Mob) state =
    if Array.exists (fun x -> x = Attack) playerInput
       then List.append state [ { location = player.location;
                                  speed = { dx = 0.0f; dy = -750.0f };
                                  texture = null } ]
       else state

/// Move bullets towards top of the screen
let private moveBullets (gameTime:GameTime) state =
    List.map (fun bullet -> { bullet with location = bullet.location + bullet.speed * timeCoeff gameTime}) state

/// Remove bullets that are out of screen boundaries
let private cullBullets state =
    List.filter (fun bullet -> bullet.location.y > 0.0f) state

/// Render a single bullet
let private renderBullet res bullet =
    res.spriteBatch.Draw(res.textures.Item "laser", Vector2(bullet.location.x - 48.0f, bullet.location.y - 48.0f), Color.White)



/// Pipeline to handle updating bullets state
let bulletsUpdater state (playerInput, (player, gameTime)) =
    spawnBullets playerInput player state 
    |> moveBullets gameTime
    |> cullBullets

/// Render given bullets state
let bulletsRenderer bullets res =
    Option.iter 
    <| List.iter (renderBullet res) 
    <| bullets
