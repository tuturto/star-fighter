module Scoring

open Microsoft.Xna.Framework
open RxNA.Renderer
open Types

let scoreUpdater scores deadEnemies =
    let score, highScore = scores
    let score' = List.fold (fun total enemy -> total + enemy.score) score deadEnemies
    (score', max score' highScore)

let scoreRenderer scores res time =
    scores
    |> Option.iter (fun x -> 
                        let score, highScore = x
                        let font = res.fonts.Item "blade-48"
                        let width = font.MeasureString(highScore.ToString())
                        res.spriteBatch.DrawString(font, score.ToString(), Vector2(10.0f, 10.0f), Color.White)
                        res.spriteBatch.DrawString(font, highScore.ToString(), Vector2(1009.0f - width.X, 10.0f) , Color.White))
