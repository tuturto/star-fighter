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

let initialEnemies res =
    List.init 10 (fun index -> { location = { x = (float32)(R.NextDouble()) * 1024.0f; 
                                              y = (float32)(R.NextDouble()) * 768.0f; }
                                 speed = { dx = 0.0f;
                                           dy = 150.0f; }
                                 texture = res.textures.Item "asteroid" })

let enemiesUpdater state (time:GameTime) =
    let collided = enemyBulletCollisions.Value
                   |> List.map (fun x -> x.Enemy)
                   |> List.filter (fun x -> x.IsSome)
                   |> List.map (fun x -> x.Value)
    state 
    |> List.filter (fun enemy -> not (List.contains enemy collided))
    |> List.map (fun enemy ->
                    let newLocation = enemy.location + enemy.speed * timeCoeff time
                    { enemy with location = if newLocation.y > 768.0f
                                               then { y = -96.0f; x = (float32)(R.NextDouble() * 1024.0) }
                                               else newLocation; })

let private renderEnemy res enemy =
    res.spriteBatch.Draw(enemy.texture, 
                         Vector2(enemy.location.x - (float32)enemy.texture.Width / 2.0f, 
                                 enemy.location.y - (float32)enemy.texture.Height / 2.0f), 
                         Color.White)

let enemiesRenderer enemies res =
    Option.iter 
    <| List.iter (renderEnemy res)
    <| enemies
