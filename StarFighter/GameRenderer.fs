module GameRenderer

open Types
open RxNA.Renderer 
open Stars
open Player

let menuRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = Menu)

let gameRunningRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = GameRunning)

type Frame =
    { player: Mob option
      starField: Mob list option
      renderResources: RenderResources option }
    static member (++) (arg1, arg2) =
        { player = match arg2.player with
                       | None -> arg1.player
                       | Some n -> arg2.player;
          starField = match arg2.starField with
                       | None -> arg1.starField
                       | Some n -> arg2.starField;
          renderResources = match arg2.renderResources with
                                | None -> arg1.renderResources
                                | Some n -> arg2.renderResources; }

let initialFrame =
    { player = None;
      starField = None;
      renderResources = None; }

let mapPlayerToFrame playerStream =
    { player = Some playerStream;
      starField = None;
      renderResources = None; }

let mapStarsToFrame starStream =
    { player = None;
      starField = Some starStream;
      renderResources = None; }

let mapRenderStreamToFrame renderStream = 
    { player = None; 
      starField = None;
      renderResources = Some renderStream; }

let gameRunningRenderer frame frameStream = 
    let newFrame = frame ++ frameStream
    match frameStream.renderResources with
        | None -> newFrame
        | Some res -> 
            starsRenderer newFrame.starField res
            playerRenderer newFrame.player res
            newFrame
