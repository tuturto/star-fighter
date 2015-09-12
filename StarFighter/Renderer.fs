module RxNA.Renderer

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open System.Reactive.Subjects


type Texture =
     | SingleFrame of texture: Texture2D
     | AnimationFrame of texture: Texture2D * duration: float
     | Animation of frames: Texture list * start: float

type TextureMap = Map<string, Texture>
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

let render (res:RenderResources) =
    res.graphics.Clear(Color.Black)
    res.spriteBatch.Begin()
    renderStream.OnNext(res)
    res.spriteBatch.End()

let loadTexture (contentManager:ContentManager) resourceName =
    SingleFrame(contentManager.Load<Texture2D>(resourceName))

let loadAnimationFrame (contentManager:ContentManager) resourceName duration =
    AnimationFrame(contentManager.Load<Texture2D>(resourceName), duration)

let rec currentFrame gameTime texture =
    match texture with
        | SingleFrame t -> t
        | Animation (frames, start) -> currentFrame gameTime <| List.last frames
        | AnimationFrame (t, _) -> t
