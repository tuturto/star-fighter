module Stars

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open RxNA.Renderer 
open GameInput
open GameRenderer

let initialStarField res = 
    List.init 250 (fun index -> { x = (float32)(R.NextDouble()) * 1024.0f; 
                                  y = (float32)(R.NextDouble()) * 768.0f; 
                                  dx = 0.0f;
                                  dy = (float32)(R.Next(1, 6)) * 0.75f;
                                  texture = res.textures.Item "star" })

let starsUpdater state (time:GameTime) =
    state |> List.map (fun star ->
        let speed = (float32)(time.ElapsedGameTime.TotalMilliseconds / 1000.0)
        let newY = star.y + star.dy * speed
        if newY > 768.0f
            then { star with y = 0.0f }
            else { star with y = star.y + star.dy })

let starsRenderer (state, (res:RenderResources)) =
    state |> List.iter 
                (fun star -> 
                    let colour = match star with
                                     | { Mob.dy = speed } when speed <= 0.75f -> Color.DarkBlue 
                                     | { Mob.dy = speed } when speed <= 1.5f -> Color.Blue       
                                     | { Mob.dy = speed } when speed <= 2.25f -> Color.CornflowerBlue  
                                     | { Mob.dy = speed } when speed <= 3.0f -> Color.LightBlue 
                                     | _ -> Color.White
                    res.spriteBatch.Draw(star.texture, Vector2(star.x, star.y), colour))
