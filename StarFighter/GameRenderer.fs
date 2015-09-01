module GameRenderer

open Types
open RxNA.Renderer 
open Stars
open Menu
open Player

let menuRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = Menu)

let gameRunningRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = GameRunning)

type Frame =
    { player: Mob option
      menu: Mob option
      starField: Mob list option
      renderResources: RenderResources option }
    static member (++) (arg1, arg2) =
        { player = match arg2.player with
                       | None -> arg1.player
                       | Some n -> arg2.player;
          menu = match arg2.menu with
                     | None -> arg1.menu
                     | Some n -> arg2.menu;
          starField = match arg2.starField with
                       | None -> arg1.starField
                       | Some n -> arg2.starField;
          renderResources = match arg2.renderResources with
                                | None -> arg1.renderResources
                                | Some n -> arg2.renderResources; }

let initialFrame =
    { player = None;
      menu = None;
      starField = None;
      renderResources = None; }

let mapMenuToFrame menuStream =
    { player = None;
      menu = Some menuStream;
      starField = None;
      renderResources = None; }

let mapPlayerToFrame playerStream =
    { player = Some playerStream;
      menu = None;
      starField = None;
      renderResources = None; }

let mapStarsToFrame starStream =
    { player = None;
      menu = None;
      starField = Some starStream;
      renderResources = None; }

let mapRenderStreamToFrame renderStream = 
    { player = None; 
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
            playerRenderer newFrame.player res
            newFrame
