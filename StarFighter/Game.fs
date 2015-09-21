module Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open FSharp.Control.Reactive 

open ExtCore.Collections

open RxNA.Renderer 
open GameInput
open GameRenderer
open Types
open Menu
open Stars
open Player
open Bullets
open Enemies
open Explosions
open PowerUps

type Game () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let contentManager = new ContentManager(this.Services, "Content")
    let mutable renderResources =
        { RxNA.Renderer.RenderResources.graphics = null;
          spriteBatch = null;
          textures = Map.empty;
          fonts = Map.empty; }

    override this.Initialize() =
        base.Initialize()
        do graphics.PreferredBackBufferWidth <- 1024
        do graphics.PreferredBackBufferHeight <- 768
        do graphics.ApplyChanges()

        let time = new GameTime()

        gameModeStream
        |> Observable.add
            (function | ExitingGame -> this.Exit()
                      | _ -> ())

        Observable.subscribe menuInputHandler menuActionStream |> ignore

        let starFrame = menuTimeStream
                        |> Observable.merge gameRunningTimeStream
                        |> Observable.scanInit (initialStarField renderResources time) starsUpdater
                        |> Observable.map mapStarsToFrame

        let menuFrame = menuTimeStream
                        |> Observable.scanInit (initialMenu renderResources time) menuUpdater
                        |> Observable.map mapMenuToFrame

        menuRenderStream
        |> Observable.map mapRenderStreamToFrame
        |> Observable.merge starFrame
        |> Observable.merge menuFrame
        |> Observable.scanInit initialFrame menuShownRenderer
        |> Observable.subscribe (fun x -> ())
        |> ignore

        let enemiesStream = gameRunningTimeStream
                            |> Observable.scanInit (initialEnemies renderResources time) (enemiesUpdater renderResources)
                            |> Observable.publish 

        let enemiesFrame = enemiesStream
                           |> Observable.map mapEnemiesToFrame

        let playerStream = gameRunningTimeStream
                           |> Observable.zip playerActionStream
                           |> Observable.zip enemiesStream
                           |> Observable.scanInit (initialPlayer renderResources time) playerUpdater
                           |> Observable.publish

        let playerFrame = playerStream
                          |> Observable.map mapPlayerToFrame                           

        let bulletStream = gameRunningTimeStream
                           |> Observable.zip playerStream
                           |> Observable.zip enemiesStream
                           |> Observable.zip playerActionStream
                           |> Observable.scanInit (initialBullets renderResources) (bulletsUpdater renderResources)
                           |> Observable.publish

        let bulletsFrame = bulletStream                           
                           |> Observable.map mapBulletsToFrame

        let powerUpStream = gameRunningTimeStream
                            |> Observable.zip deadEnemies
                            |> Observable.scanInit (initialPowerUps renderResources) (powerUpUpdater renderResources)

        let powerUpFrame = powerUpStream
                           |> Observable.map mapPowerUpsToFrame

        let explosionStream = gameRunningTimeStream
                              |> Observable.zip enemyBulletCollisions
                              |> Observable.zip deadEnemies
                              |> Observable.scanInit (initialExplosions renderResources) (explosionUpdater renderResources)

        let explosionFrame = explosionStream
                             |> Observable.map mapExplosionsToFrame

        gameRunningRenderStream
        |> Observable.map mapRenderStreamToFrame
        |> Observable.merge playerFrame
        |> Observable.merge bulletsFrame
        |> Observable.merge enemiesFrame
        |> Observable.merge starFrame
        |> Observable.merge bulletsFrame
        |> Observable.merge explosionFrame
        |> Observable.merge powerUpFrame
        |> Observable.scanInit initialFrame gameRunningRenderer
        |> Observable.subscribe (fun x -> ())
        |> ignore

        bulletStream.Connect() |> ignore
        playerActionStream.Connect() |> ignore
        gameRunningTimeStream.Connect() |> ignore
        enemiesStream.Connect() |> ignore
        playerStream.Connect() |> ignore

    override this.LoadContent() =
        let texture = loadTexture contentManager
        renderResources <-
            { RxNA.Renderer.RenderResources.graphics = this.GraphicsDevice;
              spriteBatch = new SpriteBatch(this.GraphicsDevice);
              textures = Map.empty.Add("star", texture "star")
                                  .Add("player", texture "player")
                                  .Add("asteroid", texture "asteroid")
                                  .Add("laser", texture "laser")
                                  .Add("dualshot", texture "dualshot")
                                  .Add("machinegun", texture "machinegun")
                                  .Add("shotgun", texture "shotgun")
                                  .Add("small explosion", Animation [ loadAnimationFrame contentManager "small_explosion_f1" 100.0;
                                                                      loadAnimationFrame contentManager "small_explosion_f2" 200.0;
                                                                      loadAnimationFrame contentManager "small_explosion_f3" 300.0; ])
                                  .Add("large explosion", Animation [ loadAnimationFrame contentManager "large_explosion_f1" 100.0;
                                                                      loadAnimationFrame contentManager "large_explosion_f2" 200.0;
                                                                      loadAnimationFrame contentManager "large_explosion_f3" 300.0;
                                                                      loadAnimationFrame contentManager "large_explosion_f4" 400.0;
                                                                      loadAnimationFrame contentManager "large_explosion_f5" 500.0; ])
                                  .Add("spider", LoopingAnimation [ loadAnimationFrame contentManager "spider_f1" 300.0;
                                                                    loadAnimationFrame contentManager "spider_f2" 600.0; ])  
              fonts = Map.empty.Add("blade-12", contentManager.Load<SpriteFont>("fonts/blade-12"))
                               .Add("blade-48", contentManager.Load<SpriteFont>("fonts/blade-48"))
                               .Add("blade-54", contentManager.Load<SpriteFont>("fonts/blade-54"))
                               .Add("blade-72", contentManager.Load<SpriteFont>("fonts/blade-72")) }

    override this.Update gameTime =
        RxNA.Input.mouseStateStream.OnNext(Mouse.GetState())
        RxNA.Input.keyboardStateStream.OnNext(Keyboard.GetState())
        RxNA.Input.gamePadStream.OnNext(GamePad.GetState(PlayerIndex.One))
        RxNA.Input.gameTimeStream.OnNext(gameTime)

    override this.Draw (gameTime) =
        RxNA.Renderer.render renderResources gameTime
