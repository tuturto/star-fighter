module Player

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics 

open System.Reactive
open FSharp.Control.Reactive 

open Types
open RxNA.Renderer 
open GameInput
open GameRenderer

let initialPlayer res = 
    { x = 464.0f;
      y = 600.0f;
      dx = 0.0f;
      dy = 0.0f;
      texture = res.textures.Item "player" } 

let playerUpdater state (time:GameTime) =
    state

let playerRenderer (state, (res: RenderResources)) = 
    res.spriteBatch.Draw(state.texture, Vector2(state.x, state.y), Color.White)
