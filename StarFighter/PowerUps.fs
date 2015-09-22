module PowerUps

open Microsoft.Xna.Framework 

open RxNA.Renderer 
open GameInput
open Types

let initialPowerUps res = List.empty<PowerUp>

let private randomPowerUp res time (enemy:Enemy) =
    let powerUpType = match R.Next(1, 4) with 
                      | 1 -> Machinegun
                      | 2 -> Shotgun
                      | _ -> Dualshot
    let texture = match powerUpType with
                  | Machinegun -> res.textures.Item "machinegun"
                  | Shotgun -> res.textures.Item "shotgun"
                  | Dualshot -> res.textures.Item "dualshot"
    { PowerUp.location = enemy.location; 
      speed = { dx = 0.0f; dy = 100.0f; }
      texture = convert time texture;
      weapon = powerUpType; }

let private spawnPowerUps res (state:PowerUp list) (deadEnemies:Enemy list) time =
    deadEnemies
    |> List.filter (fun dead -> R.NextDouble() > 0.90)
    |> List.map (fun dead -> randomPowerUp res time dead)

let powerUpUpdater res state ((deadEnemies:Enemy list), (time:GameTime)) =
    state
    |> List.append <| spawnPowerUps res state deadEnemies time
    |> List.map (fun powerUp -> { powerUp with location = powerUp.location + powerUp.speed * timeCoeff time })
    |> List.filter (fun powerUp -> powerUp.location.y < 1120.0f )

let private renderPowerUp res time (powerUp:PowerUp) =
    let texture = currentFrame time powerUp.texture
    res.spriteBatch.Draw(texture , 
                         Vector2(powerUp.location.x - (float32)texture.Width / 2.0f, 
                                 powerUp.location.y - (float32)texture.Height / 2.0f), 
                         Color.White)

let powerUpsRenderer powerUps res time =
    Option.iter 
    <| List.iter (renderPowerUp res time)
    <| powerUps
