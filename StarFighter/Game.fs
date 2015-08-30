module Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open FSharp.Control.Reactive 

open RxNA.Renderer 
open GameInput
open GameRenderer
open Types
open Menu
open Stars
open Player

type Game () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let contentManager = new ContentManager(this.Services, "Content")
    let mutable renderResources =
        { RxNA.Renderer.RenderResources.graphics = null;
          spriteBatch = null;
          textures = Map.empty;
          fonts = Map.empty;
          gameTime = null }

    override this.Initialize() =
        base.Initialize()
        do graphics.PreferredBackBufferWidth <- 1024
        do graphics.PreferredBackBufferHeight <- 768
        do graphics.ApplyChanges()

        gameModeStream
        |> Observable.add
            (function | ExitingGame -> this.Exit()
                      | _ -> ())

        Observable.subscribe menuInputHandler menuActionStream |> ignore

        menuRenderStream
        |> Observable.merge gameRunningRenderStream
        |> Observable.zip (menuTimeStream
                           |> Observable.merge gameRunningTimeStream
                           |> Observable.scanInit (initialStarField renderResources) starsUpdater)
        |> Observable.subscribe starsRenderer
        |> ignore

        menuRenderStream
        |> Observable.zip (menuTimeStream |> Observable.scanInit (initialMenu renderResources) menuUpdater)
        |> Observable.subscribe menuRenderer
        |> ignore

        gameRunningRenderStream
        |> Observable.zip (gameRunningTimeStream |> Observable.scanInit (initialPlayer renderResources) playerUpdater)
        |> Observable.subscribe playerRenderer
        |> ignore

    override this.LoadContent() =
        renderResources <-
            { RxNA.Renderer.RenderResources.graphics = this.GraphicsDevice;
              spriteBatch = new SpriteBatch(this.GraphicsDevice);
              textures = Map.empty.Add("star", contentManager.Load<Texture2D>("star"))
                                  .Add("player", contentManager.Load<Texture2D>("player"));
              fonts = Map.empty.Add("blade-12", contentManager.Load<SpriteFont>("fonts/blade-12"))
                               .Add("blade-48", contentManager.Load<SpriteFont>("fonts/blade-48"))
                               .Add("blade-54", contentManager.Load<SpriteFont>("fonts/blade-54"))
                               .Add("blade-72", contentManager.Load<SpriteFont>("fonts/blade-72"))                               
              gameTime = null }

    override this.Update gameTime =
        RxNA.Input.mouseStateStream.OnNext(Mouse.GetState())
        RxNA.Input.keyboardStateStream.OnNext(Keyboard.GetState())
        RxNA.Input.gamePadStream.OnNext(GamePad.GetState(PlayerIndex.One))
        RxNA.Input.gameTimeStream.OnNext(gameTime)

    override this.Draw (gameTime) =
        RxNA.Renderer.render {renderResources with gameTime = gameTime}
