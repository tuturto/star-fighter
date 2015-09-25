module RxNA.Renderer

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Audio

open System.Reactive.Subjects

type TextureSpecification =
    | Infinite of texture: Texture2D
    | Timed of texture: Texture2D * duration: float
    | Animation of frames: TextureSpecification list
    | LoopingAnimation of frames: TextureSpecification list

type Texture =
     | SingleFrame of texture: Texture2D * duration: float option
     | MultipleFrames of frames: Texture list * start: float * loop: bool 
     member this.Duration =
        match this with
            | SingleFrame (t, (Some d)) -> d
            | SingleFrame (t, None) -> 0.0
            | MultipleFrames (t, _, _) -> List.map (fun (frame:Texture) -> frame.Duration) t
                                          |> List.max

type TextureMap = Map<string, TextureSpecification>
type FontMap = Map<string, SpriteFont>
type SoundMap = Map<string, SoundEffect>

[<NoComparison>]
type RenderResources = 
    { graphics: GraphicsDevice
      spriteBatch: SpriteBatch
      textures: TextureMap
      fonts: FontMap
      sounds: SoundMap }

let renderStream =
    new Subject<RenderResources * GameTime>()

let render (res:RenderResources) (time:GameTime) =
    res.graphics.Clear(Color.Black)
    res.spriteBatch.Begin()
    renderStream.OnNext((res, time))
    res.spriteBatch.End()

let loadTexture (contentManager:ContentManager) resourceName =
    Infinite(contentManager.Load<Texture2D>(resourceName))

let loadAnimationFrame (contentManager:ContentManager) resourceName duration =
    Timed(contentManager.Load<Texture2D>(resourceName), duration)

let currentFrame (gameTime:GameTime) texture =
    match texture with
        | SingleFrame (t, d) -> t
        | MultipleFrames (frames, start, _) -> 
            let currentTime = (gameTime.TotalGameTime.TotalMilliseconds - start) % texture.Duration
            frames
            |> List.fold (fun (state:Texture option) frame -> 
                            match state with
                                | None -> if frame.Duration > currentTime
                                             then Some frame
                                             else None
                                | Some t -> Some t) None
            |> function
                   | Some (SingleFrame (t, _)) -> t
                   | _ -> failwith "failed to retrieve frame"
        
let rec convert (gameTime:GameTime) textureSpec =
    match textureSpec with
        | Infinite t -> SingleFrame (t, None)
        | Timed (t, d) -> SingleFrame (t, Some d)
        | Animation frames -> 
            let frameList = List.map (convert gameTime) frames
            MultipleFrames (frameList, gameTime.TotalGameTime.TotalMilliseconds, false)
        | LoopingAnimation frames -> 
            let frameList = List.map (convert gameTime) frames
            MultipleFrames (frameList, gameTime.TotalGameTime.TotalMilliseconds, true)

let isFinished animation (gameTime:GameTime) =
    match animation with
        | SingleFrame _ -> false
        | MultipleFrames (frames, start, false) -> gameTime.TotalGameTime.TotalMilliseconds - start > animation.Duration
        | MultipleFrames (_, _, true) -> false
