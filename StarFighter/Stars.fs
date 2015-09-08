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
                                  speed = { dx = 0.0f;
                                            dy = (float32)(R.Next(1, 6)) * 25.0f; }
                                  texture = res.textures.Item "star" })

let starsUpdater state (time:GameTime) =
    state |> List.map (fun star ->
        let newLocation = star.location + star.speed * timeCoeff time
        let newLocation' = if newLocation.y > 768.0f
                              then { newLocation with y = 0.0f }
                              else newLocation
        { star with location = newLocation' })

let private renderStar res star =
    let colour = match star with
                     | { Mob.speed = speed } when speed.dy <= 25.0f -> Color.DarkBlue 
                     | { Mob.speed = speed } when speed.dy <= 50.0f -> Color.Blue       
                     | { Mob.speed = speed } when speed.dy <= 75.0f -> Color.CornflowerBlue  
                     | { Mob.speed = speed } when speed.dy <= 100.0f -> Color.LightBlue 
                     | _ -> Color.White
    res.spriteBatch.Draw(star.texture, Vector2(star.location.x, star.location.y), colour)

let starsRenderer stars res =
    Option.iter
    <| List.iter (renderStar res)
    <| stars
