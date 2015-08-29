module RxNA.Renderer

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open System.Reactive.Subjects
open Types

type TextureMap = Map<string, Texture2D>
type FontMap = Map<string, SpriteFont>

[<NoComparison>]
type RenderResources = {
    graphics: GraphicsDevice;
    spriteBatch: SpriteBatch;
    textures: TextureMap;
    fonts: FontMap;
    gameTime: GameTime }

let renderStream =
    new Subject<RenderResources>()
 
let menuRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = Menu)

let render (res:RenderResources) =
    res.graphics.Clear(Color.Black)
    res.spriteBatch.Begin()
    renderStream.OnNext(res)
    res.spriteBatch.End()
