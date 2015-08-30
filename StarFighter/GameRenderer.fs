module GameRenderer

open Types
open RxNA.Renderer 

let menuRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = Menu)

let gameRenderStream =
    renderStream
    |> Observable.filter (fun res -> gameModeStream.Value = GameRunning)
