module Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open RxNA.Input
open RxNA.Renderer

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

        RxNA.Input.keyDownStream
        |> Observable.add
            (function | Keys.Escape -> this.Exit()
                      | _ -> ())

    override this.LoadContent() =
        renderResources <-
            { RxNA.Renderer.RenderResources.graphics = this.GraphicsDevice;
              spriteBatch = new SpriteBatch(this.GraphicsDevice);
              textures = Map.empty;
              fonts = Map.empty.Add("blade-12", contentManager.Load<SpriteFont>("fonts/blade-12"))
                               .Add("blade-48", contentManager.Load<SpriteFont>("fonts/blade-48"))
                               .Add("blade-54", contentManager.Load<SpriteFont>("fonts/blade-54"))
                               .Add("blade-72", contentManager.Load<SpriteFont>("fonts/blade-72"))
              gameTime = null }

    override this.Update gameTime =
        RxNA.Input.mouseStateStream.OnNext(Mouse.GetState())
        RxNA.Input.keyboardStateStream.OnNext(Keyboard.GetState())
        RxNA.Input.gameTimeStream.OnNext(gameTime)

    override this.Draw (gameTime) =
        RxNA.Renderer.render {renderResources with gameTime = gameTime}
