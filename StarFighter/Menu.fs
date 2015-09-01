module Menu

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

open RxNA.Renderer
open RxNA.Input 
open GameInput
open Types

let menuActionStreamKeys = keysPressedStream
                           |> Observable.filter
                                (fun x -> gameModeStream.Value = Menu)
                           |> Observable.map
                                (fun x -> x |> Array.map (function | Keys.Escape -> Some(ExitGame)
                                                                   | Keys.Space -> Some(StartGame)
                                                                   | _ -> None)
                                            |> Array.filter (fun item -> item.IsSome)
                                            |> Array.map (fun item -> item.Value))

let menuActionStreamPad = gamePadStream 
                          |> Observable.filter
                                (fun x -> gameModeStream.Value = Menu)
                          |> Observable.map
                                (fun x -> [| (if x.IsButtonDown Buttons.B then Some(ExitGame) else None);
                                             (if x.IsButtonDown Buttons.A then Some(StartGame) else None) |]
                                          |> Array.filter (fun item -> item.IsSome)
                                          |> Array.map (fun item -> item.Value))

let menuActionStream = menuActionStreamKeys
                       |> Observable.merge menuActionStreamPad 

let initialMenu res = 
    { x = 500.0f;
      y = 600.0f;
      dx = 0.0f;
      dy = 0.0f;
      texture = res.textures.Item "player" }

let menuInputHandler actions = 
    if actions |> Array.exists (fun action -> action = MenuAction.ExitGame) 
        then gameModeStream.OnNext ExitingGame
        else if actions |> Array.exists (fun action -> action = MenuAction.StartGame)
            then gameModeStream.OnNext GameRunning
            else ()

let menuUpdater state (time:GameTime) =
    state

let menuRenderer state res =
    match state with
        | None -> ()
        | Some menu ->
            res.spriteBatch.Draw(menu.texture, Vector2(menu.x, menu.y), Color.White)
            let font72 = res.fonts.Item "blade-72"
            let font12 = res.fonts.Item "blade-12"
            res.spriteBatch.DrawString(font72, "star fighter", Vector2(90.0f, 150.0f), Color.White)
            res.spriteBatch.DrawString(font12, "by tuukka turto", Vector2(410.0f, 250.0f), Color.White)
            res.spriteBatch.DrawString(font12, "press button to start", Vector2(375.0f, 450.0f), Color.White)
