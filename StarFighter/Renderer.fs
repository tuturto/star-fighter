module RxNA.Renderer

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open System.Reactive.Subjects

type TextureSpecification =
    | Infinite of texture: Texture2D
    | Timed of texture: Texture2D * duration: float
    | Animation of frames: TextureSpecification list

type Texture =
     | SingleFrame of texture: Texture2D * duration: float option
     | MultipleFrames of frames: Texture list * start: float
     member this.Duration =
        match this with
            | SingleFrame (t, d) -> d
            | MultipleFrames (_, _) -> None

type TextureMap = Map<string, TextureSpecification>
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
    Infinite(contentManager.Load<Texture2D>(resourceName))

let loadAnimationFrame (contentManager:ContentManager) resourceName duration =
    Timed(contentManager.Load<Texture2D>(resourceName), duration)

let currentFrame (gameTime:GameTime) texture =
    match texture with
        | SingleFrame (t, d) -> t
        | MultipleFrames (frames, start) -> 
            let currentTime = gameTime.TotalGameTime.TotalMilliseconds - start
            System.Diagnostics.Debug.WriteLine currentTime
            frames
            |> List.fold (fun (state:Texture option) frame -> 
                            match state with
                                | None -> if frame.Duration.Value > currentTime
                                             then Some frame
                                             else None
                                | Some t -> Some t) None
            |> function
                   | Some (SingleFrame (t, _)) -> t
                   | _ -> failwith "failed to retrieve frame"
            
        
let rec convert (time:GameTime) textureSpec =
    match textureSpec with
        | Infinite t -> SingleFrame (t, None)
        | Timed (t, d) -> SingleFrame (t, Some d)
        | Animation frames -> MultipleFrames ( List.map (fun x -> convert time x) frames, time.TotalGameTime.TotalMilliseconds )
