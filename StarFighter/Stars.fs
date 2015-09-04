module Stars

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open RxNA.Renderer 
open GameInput

let initialStarField res = 
    List.init 250 (fun index -> { location = { x = (float32)(R.NextDouble()) * 1024.0f; 
                                               y = (float32)(R.NextDouble()) * 768.0f; }
                                  dx = 0.0f;
                                  dy = (float32)(R.Next(1, 6)) * 25.0f;
                                  texture = res.textures.Item "star" })

let starsUpdater state (time:GameTime) =
    state |> List.map (fun star ->
        let speed = (float32)(time.ElapsedGameTime.TotalMilliseconds / 1000.0)
        let newY = star.location.y + star.dy * speed
        let newLocation = { x = star.location.x;
                            y = if newY > 768.0f
                                   then 0.0f
                                   else newY; }
        { star with location = newLocation })

let private renderStar res star =
    let colour = match star with
                     | { Mob.dy = speed } when speed <= 25.0f -> Color.DarkBlue 
                     | { Mob.dy = speed } when speed <= 50.0f -> Color.Blue       
                     | { Mob.dy = speed } when speed <= 75.0f -> Color.CornflowerBlue  
                     | { Mob.dy = speed } when speed <= 100.0f -> Color.LightBlue 
                     | _ -> Color.White
    res.spriteBatch.Draw(star.texture, Vector2(star.location.x, star.location.y), colour)

let starsRenderer stars res =
    Option.iter (fun state ->
                    List.iter (renderStar res) state) stars
