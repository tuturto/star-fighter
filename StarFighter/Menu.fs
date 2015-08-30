module Menu

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open RxNA.Renderer
open GameInput
open Types

let initialMenu res = 
    { x = 500.0f;
      y = 600.0f;
      dx = 0.0f;
      dy = 0.0f;
      texture = res.textures.Item "player" }

let menuInputHandler actions = 
    if actions |> Array.exists (fun action -> action = MenuAction.ExitGame) 
        then gameModeStream.OnNext ExitingGame
        else ()

let menuUpdater state (time:GameTime) =
    state

let menuRenderer (state, (res:RenderResources)) =
    res.spriteBatch.Draw(state.texture, Vector2(state.x, state.y), Color.White)
    let font72 = res.fonts.Item "blade-72"
    let font12 = res.fonts.Item "blade-12"
    res.spriteBatch.DrawString(font72, "star fighter", Vector2(90.0f, 150.0f), Color.White)
    res.spriteBatch.DrawString(font12, "by tuukka turto", Vector2(410.0f, 250.0f), Color.White)
    res.spriteBatch.DrawString(font12, "press button to start", Vector2(375.0f, 450.0f), Color.White)
