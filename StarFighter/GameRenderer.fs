module GameRenderer

open Types
open RxNA.Renderer 
open Stars
open Menu
open Player
open Enemies

let menuRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = Menu)

let gameRunningRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = GameRunning)

let choose opt1 opt2 =
    match opt2 with
        | None -> opt1
        | Some n -> opt2

type Frame =
    { player: Mob option
      enemies: Mob list option
      menu: Mob option
      starField: Mob list option
      renderResources: RenderResources option }
    static member (++) (arg1, arg2) =
        { player = choose arg1.player arg2.player;
          enemies = choose arg1.enemies arg2.enemies;
          menu = choose arg1.menu arg2.menu;
          starField = choose arg1.starField arg2.starField;
          renderResources = choose arg1.renderResources arg2.renderResources; }

let initialFrame =
    { player = None;
      enemies = None;
      menu = None;
      starField = None;
      renderResources = None; }

let mapMenuToFrame menuStream =
    { player = None;
      enemies = None;
      menu = Some menuStream;
      starField = None;
      renderResources = None; }

let mapPlayerToFrame playerStream =
    { player = Some playerStream;
      enemies = None;
      menu = None;
      starField = None;
      renderResources = None; }

let mapStarsToFrame starStream =
    { player = None;
      enemies = None;
      menu = None;
      starField = Some starStream;
      renderResources = None; }

let mapEnemiesToFrame enemiesStream =
    { player = None; 
      enemies = Some enemiesStream;
      menu = None;
      starField = None;
      renderResources = None; }    

let mapRenderStreamToFrame renderStream = 
    { player = None; 
      enemies = None;
      menu = None;
      starField = None;
      renderResources = Some renderStream; }

let menuShownRenderer frame frameStream =     
    let newFrame = frame ++ frameStream
    match frameStream.renderResources with
        | None -> newFrame
        | Some res -> 
            starsRenderer newFrame.starField res
            menuRenderer newFrame.menu res
            newFrame

let gameRunningRenderer frame frameStream = 
    let newFrame = frame ++ frameStream
    match frameStream.renderResources with
        | None -> newFrame
        | Some res -> 
            starsRenderer newFrame.starField res
            enemiesRenderer newFrame.enemies res
            playerRenderer newFrame.player res
            newFrame
