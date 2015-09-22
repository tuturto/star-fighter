module Enemies

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open RxNA.Renderer 
open RxNA.Input 
open GameInput

let initialEnemies res time =
    List.init 10 (fun index -> { location = { x = (float32)(R.NextDouble()) * 1024.0f; 
                                              y = (float32)(R.NextDouble()) * 400.0f; }
                                 speed = { dx = 0.0f;
                                           dy = 150.0f; }
                                 texture = convert time <| res.textures.Item "asteroid";
                                 hp = 10; })

let randomEnemy res time =
    match R.Next(1, 6) with
        | 1 -> { location = { x = -96.0f; 
                              y = (float32)(R.NextDouble()) * 568.0f; }
                 speed = { dx = 150.0f;
                           dy = 50.0f; }
                 texture = convert time <| res.textures.Item "spider"
                 hp = 3; }
        | 2 -> { location = { x = 1120.0f; 
                              y = (float32)(R.NextDouble()) * 568.0f; }
                 speed = { dx = -150.0f;
                           dy = 50.0f; }
                 texture = convert time <| res.textures.Item "spider"
                 hp = 3; }
        | _ -> { location = { x = (float32)(R.NextDouble()) * 1024.0f; 
                              y = -96.0f; }
                 speed = { dx = 0.0f;
                           dy = 150.0f; }
                 texture = convert time <| res.textures.Item "asteroid";
                 hp = 10; }

let spawnEnemies res time state =
    if List.length state > 10
       then state
       else List.cons state <| randomEnemy res time

let enemiesUpdater res (state:Enemy list) (time:GameTime) =
    let collided = enemyBulletCollisions.Value
                   |> List.map (fun x -> x.Enemy)
                   |> List.filter (fun x -> x.IsSome)
                   |> List.map (fun x -> x.Value)
    let dead = collided
               |> List.filter (fun x -> x.hp < 2)
    deadEnemies.OnNext dead
    state 
    |> List.filter (fun enemy -> not (List.contains enemy dead))
    |> List.map (fun enemy -> if (List.contains enemy collided)
                                  then { enemy with hp = enemy.hp - 1 }
                                  else enemy)
    |> List.map (fun enemy -> { enemy with location = enemy.location + enemy.speed * timeCoeff time })
    |> List.map (fun enemy -> 
                    if enemy.location.y > 768.0f || enemy.location.x < -96.0f || enemy.location.x > 1120.0f
                       then randomEnemy res time
                       else enemy)
    |> spawnEnemies res time

let private renderEnemy res time (enemy:Enemy) =
    let texture = currentFrame time enemy.texture
    res.spriteBatch.Draw(texture , 
                         Vector2(enemy.location.x - (float32)texture.Width / 2.0f, 
                                 enemy.location.y - (float32)texture.Height / 2.0f), 
                         Color.White)

let enemiesRenderer (enemies:Enemy list option) res time =
    Option.iter 
    <| List.iter (renderEnemy res time)
    <| enemies
