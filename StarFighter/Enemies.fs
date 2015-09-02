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
    List.init 10 (fun index -> { x = (float32)(R.NextDouble()) * 1024.0f; 
                                  y = (float32)(R.NextDouble()) * 768.0f; 
                                  dx = 0.0f;
                                  dy = 5.0f;
                                  texture = res.textures.Item "asteroid" })

let enemiesUpdater state (player, (time:GameTime)) =
    state |> List.map (fun enemy ->
        let speed = (float32)(time.ElapsedGameTime.TotalMilliseconds / 1000.0)
        let newY = enemy.y + enemy.dy * speed
        if newY > 768.0f
            then { enemy with y = -96.0f; x = (float32)(R.NextDouble() * 1024.0) }
            else { enemy with y = enemy.y + enemy.dy })

let private renderEnemy res enemy =
    res.spriteBatch.Draw(enemy.texture, Vector2(enemy.x - 48.0f, enemy.y - 48.0f), Color.White)

let enemiesRenderer enemies res =
    match enemies with
        | None -> ()
        | Some state -> 
            List.iter (renderEnemy res) state
