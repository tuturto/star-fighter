module ReadyScreen

open Microsoft.Xna.Framework 
open RxNA.Renderer
open Types
open ExtCore.Collections

type ReadyFrame = { time: GameTime option
                    mode: GameMode option }

let mapTimeToReadyScreen time =
    { time = Some time
      mode = None }

let mapModeToReadyScreen mode =
    { time = None
      mode = Some mode }

type readyState = { startTime: float option
                    mode: GameMode option }

let initialReadyState = { startTime = None
                          mode = None }

let private inactiveState = { startTime = None
                              mode = None }

let delayElapsed gameTime startTime =
    Option.lift2 (fun (time:GameTime) startTime -> 
                      time.TotalGameTime.TotalMilliseconds > startTime + 3000.0) gameTime startTime
    |> function
       | None -> false
       | Some t -> t

let readyScreenUpdater newGameMode (state:readyState) (frame:ReadyFrame) =
    match frame.time with
        | Some time -> if delayElapsed frame.time state.startTime 
                          then newGameMode ExitingGame
                               inactiveState
                          else if state.startTime.IsNone 
                                  then { state with startTime = Some time.TotalGameTime.TotalMilliseconds }
                                  else state
        | None -> match frame.mode with
                      | Some Menu -> inactiveState
                      | Some GameRunning -> inactiveState
                      | Some ReadyScreen -> { startTime = None; mode = Some ReadyScreen }
                      | Some ExitingGame -> inactiveState
                      | None -> inactiveState

let readyTextRenderer res time player = 
    Option.iter (fun player ->
                     let font = res.fonts.Item "blade-12"
                     res.spriteBatch.DrawString(font, "Game Over!", Vector2(455.0f, 300.0f), Color.White)) player
