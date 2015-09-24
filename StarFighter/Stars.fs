module Stars

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open RxNA.Renderer 
open GameInput

let initialStarField res (rng:System.Random) time = 
    List.init 250 (fun index -> { Mob.location = { x = (float32)(rng.NextDouble()) * 1024.0f; 
                                                   y = (float32)(rng.NextDouble()) * 768.0f; }
                                  speed = { dx = 0.0f;
                                            dy = (float32)(rng.Next(1, 6)) * 25.0f; }
                                  texture = convert time <| res.textures.Item "star"; })

let starsUpdater (state:Mob list) (time:GameTime) =
    state |> List.map (fun star ->
        let newLocation = star.location + star.speed * timeCoeff time
        let newLocation' = if newLocation.y > 768.0f
                              then { newLocation with y = 0.0f }
                              else newLocation
        { star with location = newLocation' })

let private renderStar res time star =
    let colour = match star with
                     | { Mob.speed = speed } when speed.dy <= 25.0f -> Color.DarkBlue 
                     | { Mob.speed = speed } when speed.dy <= 50.0f -> Color.Blue       
                     | { Mob.speed = speed } when speed.dy <= 75.0f -> Color.CornflowerBlue  
                     | { Mob.speed = speed } when speed.dy <= 100.0f -> Color.LightBlue 
                     | _ -> Color.White
    res.spriteBatch.Draw(currentFrame time star.texture, Vector2(star.location.x, star.location.y), colour)

let starsRenderer stars res time =
    Option.iter
    <| List.iter (renderStar res time)
    <| stars
