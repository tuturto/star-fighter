module Game

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Audio 
open Microsoft.Xna.Framework.Input

open FSharp.Control.Reactive 
open System.Reactive.Subjects
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
open Scoring
open Sounds

type Game () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let contentManager = new ContentManager(this.Services, "Content")
    let mutable renderResources =
        { RxNA.Renderer.RenderResources.graphics = null
          spriteBatch = null
          textures = Map.empty
          fonts = Map.empty 
          sounds = Map.empty }

    override this.Initialize() =
        base.Initialize()
        do graphics.PreferredBackBufferWidth <- 1024
        do graphics.PreferredBackBufferHeight <- 768
        do graphics.ApplyChanges()

        let time = new GameTime()

        let enemyBulletCollisions = new BehaviorSubject<BulletCollisionInfo list>([])
        let playerPowerUpCollisions = new BehaviorSubject<PowerUp list>([])
        let deadEnemies = new BehaviorSubject<Enemy list>([])
        let soundStream = new BehaviorSubject<SoundEvent>(Silence)
        let rng = System.Random()        

        gameModeStream
        |> Observable.add
            (function | ExitingGame -> this.Exit()
                      | _ -> ())

        Observable.subscribe menuInputHandler menuActionStream |> ignore

        let starFrame = menuTimeStream
                        |> Observable.merge gameRunningTimeStream
                        |> Observable.scanInit (initialStarField renderResources rng time) starsUpdater
                        |> Observable.map mapStarsToFrame

        let enemiesStream = gameRunningTimeStream
                            |> Observable.scanInit (initialEnemies renderResources rng time) (enemiesUpdater deadEnemies.OnNext enemyBulletCollisions rng renderResources)
                            |> Observable.publish 

        let playerStream = gameRunningTimeStream
                           |> Observable.zip playerActionStream
                           |> Observable.zip enemiesStream
                           |> Observable.scanInit (initialPlayer renderResources time) (playerUpdater soundStream.OnNext)
                           |> Observable.publish

        playerStream
        |> Observable.zip gameRunningTimeStream
        |> Observable.add (fun (time, player) -> 
                                match player with
                                    | NormalPlayer _ -> ()
                                    | ExplodingPlayer info -> if info.explosionTime + 3000.0 < time.TotalGameTime.TotalMilliseconds 
                                                                 then gameModeStream.OnNext ReadyScreen
                                                                 else ())

        let powerUpStream = gameRunningTimeStream
                            |> Observable.zip deadEnemies
                            |> Observable.scanInit (initialPowerUps renderResources) (powerUpUpdater renderResources rng playerPowerUpCollisions)
                            |> Observable.publish 

        let bulletStream = gameRunningTimeStream
                           |> Observable.zip playerStream
                           |> Observable.zip enemiesStream
                           |> Observable.zip playerActionStream
                           |> Observable.zip powerUpStream
                           |> Observable.scanInit (initialBullets renderResources) 
                                                  (bulletsUpdater enemyBulletCollisions.OnNext playerPowerUpCollisions.OnNext soundStream.OnNext rng renderResources)
                           |> Observable.publish

        let explosionStream = gameRunningTimeStream
                              |> Observable.zip enemyBulletCollisions
                              |> Observable.zip deadEnemies
                              |> Observable.zip playerStream
                              |> Observable.scanInit (initialExplosions renderResources) (explosionUpdater renderResources rng soundStream.OnNext)

        let scoreStream = deadEnemies
                          |> Observable.scanInit (0, 300) scoreUpdater

        let scoreFrame = Observable.map mapScoreToFrame scoreStream

        menuRenderStream
        |> Observable.map mapRenderStreamToFrame
        |> Observable.merge starFrame
        |> Observable.merge scoreFrame
        |> Observable.scanInit initialFrame menuShownRenderer
        |> Observable.subscribe (fun x -> ())
        |> ignore

        gameRunningRenderStream
        |> Observable.map mapRenderStreamToFrame
        |> Observable.merge <| Observable.map mapPlayerToFrame playerStream
        |> Observable.merge <| Observable.map mapEnemiesToFrame enemiesStream
        |> Observable.merge starFrame
        |> Observable.merge <| Observable.map mapBulletsToFrame bulletStream
        |> Observable.merge <| Observable.map mapExplosionsToFrame explosionStream
        |> Observable.merge <| Observable.map mapPowerUpsToFrame powerUpStream
        |> Observable.merge <| scoreFrame
        |> Observable.scanInit initialFrame gameRunningRenderer
        |> Observable.subscribe (fun x -> ())
        |> ignore

        readyScreenRenderStream
        |> Observable.map mapRenderStreamToFrame
        |> Observable.merge <| Observable.map mapPlayerToFrame playerStream
        |> Observable.merge <| scoreFrame
        |> Observable.scanInit initialFrame readyScreenRenderer
        |> Observable.subscribe (fun x -> ())
        |> ignore

        soundStream
        |> Observable.map (soundUpdater renderResources)
        |> Observable.subscribe (fun x -> ())
        |> ignore

        bulletStream.Connect() |> ignore
        playerActionStream.Connect() |> ignore
        gameRunningTimeStream.Connect() |> ignore
        enemiesStream.Connect() |> ignore
        playerStream.Connect() |> ignore
        powerUpStream.Connect() |> ignore

    override this.LoadContent() =
        let texture = loadTexture contentManager
        let frame = loadAnimationFrame contentManager
        let font = loadFont contentManager
        let sfx = loadSfx contentManager
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
                                  .Add("small explosion", Animation [ frame "small_explosion_f1" 100.0;
                                                                      frame "small_explosion_f2" 200.0;
                                                                      frame "small_explosion_f3" 300.0; ])
                                  .Add("large explosion", Animation [ frame "large_explosion_f1" 100.0;
                                                                      frame "large_explosion_f2" 200.0;
                                                                      frame "large_explosion_f3" 300.0;
                                                                      frame "large_explosion_f4" 400.0;
                                                                      frame "large_explosion_f5" 500.0; ])
                                  .Add("spider", LoopingAnimation [ frame "spider_f1" 300.0;
                                                                    frame "spider_f2" 600.0; ])  
              fonts = Map.empty.Add("blade-12", font "fonts/blade-12")
                               .Add("blade-48", font "fonts/blade-48")
                               .Add("blade-54", font "fonts/blade-54")
                               .Add("blade-72", font "fonts/blade-72")
              sounds = Map.empty.Add("explosion", sfx "audio/destroyed_explosion")
                                .Add("power up", sfx "audio/powerup")
                                .Add("dualshot", sfx "audio/shoot_laser")
                                .Add("machinegun", sfx "audio/shoot_machinegun")
                                .Add("shotgun", sfx "audio/shoot_shotgun") }

    override this.Update gameTime =
        RxNA.Input.mouseStateStream.OnNext(Mouse.GetState())
        RxNA.Input.keyboardStateStream.OnNext(Keyboard.GetState())
        RxNA.Input.gamePadStream.OnNext(GamePad.GetState(PlayerIndex.One))
        RxNA.Input.gameTimeStream.OnNext(gameTime)

    override this.Draw (gameTime) =
        RxNA.Renderer.render renderResources gameTime
