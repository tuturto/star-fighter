module Menu

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open RxNA.Input 
open RxNA.Renderer 
open GameInput
open Types

let menuInputHandler = 
    menuActionStream
    |> Observable.subscribe
        (fun x -> if x |> Array.exists (fun x' -> x' = MenuAction.ExitGame) then gameModeStream.OnNext ExitingGame else ())

let menuRenderer =
    menuRenderStream
    |> Observable.subscribe
        (fun res -> 
            let font72 = res.fonts.Item "blade-72"
            let font12 = res.fonts.Item "blade-12"
            res.spriteBatch.DrawString(font72, "star fighter", Vector2(90.0f, 150.0f), Color.White)
            res.spriteBatch.DrawString(font12, "by tuukka turto", Vector2(410.0f, 250.0f), Color.White)
            res.spriteBatch.DrawString(font12, "press button to start", Vector2(375.0f, 450.0f), Color.White))
