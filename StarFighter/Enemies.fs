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
    state |> List.map (fun enemy ->
        let speed = (float32)(time.ElapsedGameTime.TotalMilliseconds / 1000.0)
        let newLocation = enemy.location + enemy.speed * speed
        { enemy with location = if newLocation.y > 768.0f
                                   then { y = -96.0f; x = (float32)(R.NextDouble() * 1024.0) }
                                   else newLocation; })

let private renderEnemy res enemy =
    res.spriteBatch.Draw(enemy.texture, Vector2(enemy.location.x - 48.0f, enemy.location.y - 48.0f), Color.White)

let enemiesRenderer enemies res =
    Option.iter (fun state ->
                    List.iter (renderEnemy res) state) enemies
