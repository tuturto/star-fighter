module Bullets

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 
open ExtCore.Collections

open RxNA.Renderer
open Types
open Collisions
open GameInput

let initialBullets renderResources = 
    { fired = 0.0;
      bullets = List.empty<Mob>;
      weapon = Machinegun }

let private spawnMachinegun res playSound (rng:System.Random) (gameTime:GameTime) (player:Mob) state =
    if (gameTime.TotalGameTime.TotalMilliseconds - state.fired) > 100.0
       then 
            playSound MachinegunFired
            { fired = gameTime.TotalGameTime.TotalMilliseconds;
              bullets = List.append state.bullets [ { location = player.location;
                                                      speed = { dx = (float32)(rng.NextDouble() * 200.0 - 100.0)
                                                                dy = -750.0f };
                                                      texture = convert gameTime <| res.textures.Item "laser"; } ] 
              weapon = state.weapon; }
       else state

let private spawnShotgun res playSound (rng:System.Random) (gameTime:GameTime) (player:Mob) state =
    let angle = System.Math.PI / 12.0
    if (gameTime.TotalGameTime.TotalMilliseconds - state.fired) > 900.0
       then 
            playSound ShotgunFired
            { fired = gameTime.TotalGameTime.TotalMilliseconds;
              bullets = List.append state.bullets <| List.init 20 (fun x -> let shotAngle = rng.NextDouble() * 2.0 * angle - (7.0 * angle);
                                                                            let speed = rng.NextDouble() * 100.0 + 300.0
                                                                            let dx = (float32)(System.Math.Cos(shotAngle) * speed) + player.speed.dx
                                                                            let dy = (float32)(System.Math.Sin(shotAngle) * speed) + player.speed.dy
                                                                            { location = player.location;
                                                                              speed = { dx = (float32)dx;
                                                                                        dy = (float32)dy };
                                                                              texture = convert gameTime <| res.textures.Item "star"; })
              weapon = state.weapon; }
       else state
       
let private spawnDualshot res playSound (rng:System.Random) (gameTime:GameTime) (player:Mob) state =
    if (gameTime.TotalGameTime.TotalMilliseconds - state.fired) > 150.0
       then 
            playSound DualshotFired       
            { fired = gameTime.TotalGameTime.TotalMilliseconds;
              bullets = List.append state.bullets [ { location = { x = player.location.x - 40.0f; y = player.location.y + 10.0f };
                                                      speed = { dx = (float32)(rng.NextDouble() * 100.0 - 50.0)
                                                                dy = -750.0f };
                                                      texture = convert gameTime <| res.textures.Item "laser"; };
                                                    { location = { x = player.location.x + 40.0f; y = player.location.y + 10.0f };
                                                      speed = { dx = (float32)(rng.NextDouble() * 100.0 - 50.0)
                                                                dy = -750.0f };
                                                      texture = convert gameTime <| res.textures.Item "laser"; } ] 
              weapon = state.weapon; }
       else state

/// Spawn new bullets if player is currently shooting
let private spawnBullets res playSound rng gameTime (playerInput:GameAction []) player state =
    match player with 
        | NormalPlayer ship ->
                if Array.exists (fun x -> x = Attack) playerInput
                   then match state.weapon with
                        | Machinegun -> spawnMachinegun res playSound rng gameTime ship state
                        | Shotgun -> spawnShotgun res playSound rng gameTime ship state
                        | Dualshot -> spawnDualshot res playSound rng gameTime ship state
                else state
        | ExplodingPlayer _ -> state

/// Render a single bullet
let private renderBullet res time (bullet:Mob) =
    let texture = currentFrame time bullet.texture 
    res.spriteBatch.Draw(texture, Vector2(bullet.location.x - (float32)texture.Width / 2.0f, bullet.location.y - (float32)texture.Height / 2.0f), Color.White)

/// Does given bullet intersect with any of the enemies?
let private isHit gameTime (bullet:Mob) (enemies:Mob list) =
    not (List.map (fun enemy -> collision gameTime bullet enemy) enemies
         |> List.filter Option.isSome
         |> List.isEmpty)

let private checkEnemyCollisions gameTime enemies bullet =
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

let private checkPowerUpCollisions playerPowerUpCollisionReport playSound res gameTime player (powerUps:PowerUp list) (state:BulletInfo) =
    match player with
        | NormalPlayer ship ->
                let collisions = powerUps
                                 |> List.filter (fun powerUp ->
                                                    let coll = collision gameTime powerUp ship
                                                    coll.IsSome)
                                 |> tap playerPowerUpCollisionReport
                if collisions.IsEmpty
                   then state
                   else playSound PowerUpCollected
                        { state with weapon = collisions.Head.weapon }
        | ExplodingPlayer _ -> state

/// Pipeline to handle updating bullets state
let bulletsUpdater enemyBulletCollisionReport playerPowerUpCollisionReport playSound rng res state (powerUps, (playerInput, (enemies, (player, gameTime)))) =
    let bullets = checkPowerUpCollisions playerPowerUpCollisionReport playSound res gameTime player powerUps state
                  |> spawnBullets res playSound rng gameTime playerInput player
    { bullets = bullets.bullets
                |> List.map (fun bullet -> { bullet with location = bullet.location + bullet.speed * timeCoeff gameTime})
                |> List.filter (fun bullet -> bullet.location.y > 0.0f)
                |> List.map (checkEnemyCollisions gameTime enemies)
                |> tap (fun x -> List.filter (function
                                                  | NoCollision _ -> false
                                                  | EnemyCollision _ -> true) x
                                 |> enemyBulletCollisionReport)
                |> List.filter (fun bullet -> not (bullet.Collided))
                |> List.map (fun bullet -> bullet.Bullet);
      fired = bullets.fired;
      weapon = bullets.weapon }

/// Render given bullets state
let bulletsRenderer (bullets:Mob list option) res time =
    Option.iter 
    <| List.iter (renderBullet res time) 
    <| bullets
