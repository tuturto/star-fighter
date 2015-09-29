module GameRenderer

open Microsoft.Xna.Framework 

open Types
open RxNA.Renderer 
open Stars
open Menu
open Player
open Enemies
open Bullets
open Explosions
open PowerUps
open Scoring
open ReadyScreen

let menuRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = Menu)

let gameRunningRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = GameRunning)

let readyScreenRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = ReadyScreen)

type Frame =
    { player: Player option
      enemies: Enemy list option
      bullets: Mob list option
      explosions: Mob list option
      powerUps: PowerUp list option
      menu: Mob option
      starField: Mob list option
      renderResources: RenderResources option
      time: GameTime option
      score: (int * int) option }
    static member (++) (arg1, arg2) =
        { player = Option.orElse arg1.player arg2.player
          enemies = Option.orElse arg1.enemies arg2.enemies
          bullets = Option.orElse arg1.bullets arg2.bullets
          explosions = Option.orElse arg1.explosions arg2.explosions
          powerUps = Option.orElse arg1.powerUps arg2.powerUps
          menu = Option.orElse arg1.menu arg2.menu
          starField = Option.orElse arg1.starField arg2.starField
          renderResources = Option.orElse arg1.renderResources arg2.renderResources
          time = Option.orElse arg1.time arg2.time
          score = Option.orElse arg1.score arg2.score }

let initialFrame =
    { player = None
      enemies = None
      bullets = None
      explosions = None
      powerUps = None
      menu = None
      starField = None
      renderResources = None
      time = None
      score = None }

let mapMenuToFrame menuStream =
    { player = None
      enemies = None
      bullets = None
      explosions = None
      powerUps = None
      menu = Some menuStream
      starField = None
      renderResources = None;
      time = None
      score = None }

let mapPlayerToFrame playerStream =
    { player = Some playerStream
      enemies = None
      bullets = None
      explosions = None
      powerUps = None
      menu = None
      starField = None
      renderResources = None;
      time = None
      score = None }

let mapStarsToFrame starStream =
    { player = None
      enemies = None
      bullets = None
      explosions = None
      powerUps = None
      menu = None
      starField = Some starStream
      renderResources = None
      time = None
      score = None }

let mapEnemiesToFrame enemiesStream =
    { player = None
      enemies = Some enemiesStream
      bullets = None
      explosions = None
      powerUps = None
      menu = None
      starField = None
      renderResources = None
      time = None
      score = None }    

let mapBulletsToFrame (bulletsStream:BulletInfo) =
    { player = None 
      enemies = None
      bullets = Some bulletsStream.bullets
      explosions = None
      powerUps = None
      menu = None
      starField = None
      renderResources = None
      time = None
      score = None }    

let mapExplosionsToFrame explosionStream =
    { player = None 
      enemies = None
      bullets = None
      explosions = Some explosionStream
      powerUps = None
      menu = None
      starField = None
      renderResources = None
      time = None
      score = None }    

let mapPowerUpsToFrame powerUpStream =
    { player = None 
      enemies = None
      bullets = None
      explosions = None
      powerUps = Some powerUpStream
      menu = None
      starField = None
      renderResources = None
      time = None
      score = None }    

let mapScoreToFrame scoreStream =
    { player = None 
      enemies = None
      bullets = None
      explosions = None
      powerUps = None
      menu = None
      starField = None
      renderResources = None
      time = None
      score = Some scoreStream }   

let mapRenderStreamToFrame (renderStream, gameTime) =
    { player = None
      enemies = None
      bullets = None
      explosions = None
      powerUps = None
      menu = None
      starField = None
      renderResources = Some renderStream
      time = Some gameTime
      score = None }

let menuShownRenderer frame frameStream =     
    let newFrame = frame ++ frameStream
    let time = new Microsoft.Xna.Framework.GameTime()
    Option.iter (fun res ->
                    starsRenderer newFrame.starField res time
                    menuRenderer res time
                    scoreRenderer newFrame.score res time) frameStream.renderResources           
    newFrame

let gameRunningRenderer frame frameStream = 
    let newFrame = frame ++ frameStream
    if frame.time.IsSome       
       then Option.iter (fun res ->
                            let time = frame.time.Value 
                            starsRenderer newFrame.starField res time
                            enemiesRenderer newFrame.enemies res time
                            explosionRenderer newFrame.explosions res time
                            bulletsRenderer newFrame.bullets res time
                            playerRenderer newFrame.player res time
                            powerUpsRenderer newFrame.powerUps res time
                            scoreRenderer newFrame.score res time) frameStream.renderResources
        else ()
    newFrame

let readyScreenRenderer frame frameStream =
    let newFrame = frame ++ frameStream
    if frame.time.IsSome
       then Option.iter (fun res -> 
                             let time = frame.time.Value 
                             readyTextRenderer res time newFrame.player
                             scoreRenderer newFrame.score res time) frameStream.renderResources
       else ()
    newFrame
