﻿module Game

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
open Bullets
open Enemies

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

        let starFrame = menuTimeStream
                        |> Observable.merge gameRunningTimeStream
                        |> Observable.scanInit (initialStarField renderResources) starsUpdater
                        |> Observable.map mapStarsToFrame

        let menuFrame = menuTimeStream
                        |> Observable.scanInit (initialMenu renderResources) menuUpdater
                        |> Observable.map mapMenuToFrame

        menuRenderStream
        |> Observable.map mapRenderStreamToFrame
        |> Observable.merge starFrame
        |> Observable.merge menuFrame
        |> Observable.scanInit initialFrame menuShownRenderer
        |> Observable.subscribe (fun x -> ())
        |> ignore

        let enemiesStream = gameRunningTimeStream
                            |> Observable.scanInit (initialEnemies renderResources) enemiesUpdater
                            |> Observable.publish 

        let enemiesFrame = enemiesStream
                           |> Observable.map mapEnemiesToFrame

        let playerStream = gameRunningTimeStream
                           |> Observable.zip playerActionStream
                           |> Observable.zip enemiesStream
                           |> Observable.scanInit (initialPlayer renderResources) playerUpdater
                           |> Observable.publish

        let playerFrame = playerStream
                          |> Observable.map mapPlayerToFrame                           

        let bulletStream = gameRunningTimeStream
                           |> Observable.zip playerStream
                           |> Observable.zip enemiesStream
                           |> Observable.zip playerActionStream
                           |> Observable.scanInit (initialBullets renderResources) (bulletsUpdater renderResources)

        let bulletsFrame = bulletStream
                           |> Observable.map mapBulletsToFrame

        gameRunningRenderStream
        |> Observable.map mapRenderStreamToFrame
        |> Observable.merge playerFrame
        |> Observable.merge bulletsFrame
        |> Observable.merge enemiesFrame
        |> Observable.merge starFrame
        |> Observable.merge bulletsFrame
        |> Observable.scanInit initialFrame gameRunningRenderer
        |> Observable.subscribe (fun x -> ())
        |> ignore

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
                                  .Add("small_explosion_f1", texture "small_explosion_f1")
                                  .Add("small_explosion_f2", texture "small_explosion_f2")
                                  .Add("small_explosion_f3", texture "small_explosion_f3")
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
