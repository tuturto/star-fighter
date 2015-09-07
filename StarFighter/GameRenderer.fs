module GameRenderer

open Types
open RxNA.Renderer 
open Stars
open Menu
open Player
open Enemies
open Bullets

let menuRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = Menu)

let gameRunningRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = GameRunning)

type Frame =
    { player: Mob option
      enemies: Mob list option
      bullets: Mob list option
      menu: Mob option
      starField: Mob list option
      renderResources: RenderResources option }
    static member (++) (arg1, arg2) =
        { player = Option.orElse arg1.player arg2.player;
          enemies = Option.orElse arg1.enemies arg2.enemies;
          bullets = Option.orElse arg1.bullets arg2.bullets;
          menu = Option.orElse arg1.menu arg2.menu;
          starField = Option.orElse arg1.starField arg2.starField;
          renderResources = Option.orElse arg1.renderResources arg2.renderResources; }

let initialFrame =
    { player = None;
      enemies = None;
      bullets = None;
      menu = None;
      starField = None;
      renderResources = None; }

let mapMenuToFrame menuStream =
    { player = None;
      enemies = None;
      bullets = None;
      menu = Some menuStream;
      starField = None;
      renderResources = None; }

let mapPlayerToFrame playerStream =
    { player = Some playerStream;
      enemies = None;
      bullets = None;
      menu = None;
      starField = None;
      renderResources = None; }

let mapStarsToFrame starStream =
    { player = None;
      enemies = None;
      bullets = None;
      menu = None;
      starField = Some starStream;
      renderResources = None; }

let mapEnemiesToFrame enemiesStream =
    { player = None; 
      enemies = Some enemiesStream;
      bullets = None;
      menu = None;
      starField = None;
      renderResources = None; }    

let mapBulletsToFrame bulletsStream =
    { player = None; 
      enemies = None;
      bullets = Some bulletsStream;
      menu = None;
      starField = None;
      renderResources = None; }    

let mapRenderStreamToFrame renderStream = 
    { player = None; 
      enemies = None;
      bullets = None;
      menu = None;
      starField = None;
      renderResources = Some renderStream; }

let menuShownRenderer frame frameStream =     
    let newFrame = frame ++ frameStream
    Option.iter (fun res ->
                    starsRenderer newFrame.starField res
                    menuRenderer newFrame.menu res) frameStream.renderResources           
    newFrame

let gameRunningRenderer frame frameStream = 
    let newFrame = frame ++ frameStream
    Option.iter (fun res ->
                    starsRenderer newFrame.starField res
                    enemiesRenderer newFrame.enemies res
                    bulletsRenderer newFrame.bullets res
                    playerRenderer newFrame.player res) frameStream.renderResources
    newFrame
