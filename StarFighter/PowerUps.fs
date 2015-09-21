module PowerUps

open Microsoft.Xna.Framework 

open RxNA.Renderer 
open GameInput
open Types

let initialPowerUps res = List.empty

let randomTexture res time =
    match R.Next(1, 4) with
        | 1 -> res.textures.Item "machinegun"
        | 2 -> res.textures.Item "shotgun"
        | _ -> res.textures.Item "dualshot"
    |> convert time

let spawnPowerUps res state deadEnemies time =
    deadEnemies
    |> List.filter (fun dead -> R.NextDouble() > 0.90)
    |> List.map (fun dead -> { dead with texture = randomTexture res time;
                                         speed = { dx = 0.0f; dy = 100.0f; } })

let powerUpUpdater res state (deadEnemies, (time:GameTime)) =
    state
    |> List.append <| spawnPowerUps res state deadEnemies time
    |> List.map (fun powerUp -> { powerUp with location = powerUp.location + powerUp.speed * timeCoeff time })
    |> List.filter (fun powerUp -> powerUp.location.y < 1120.0f )

let private renderPowerUp res time powerUp =
    let texture = currentFrame time powerUp.texture
    res.spriteBatch.Draw(texture , 
                         Vector2(powerUp.location.x - (float32)texture.Width / 2.0f, 
                                 powerUp.location.y - (float32)texture.Height / 2.0f), 
                         Color.White)

let powerUpsRenderer powerUps res time =
    Option.iter 
    <| List.iter (renderPowerUp res time)
    <| powerUps
