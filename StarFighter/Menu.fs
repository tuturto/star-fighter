module Menu

open GameInput
open Types

let menuInputHandler = 
    menuActionStream
    |> Observable.add
        (fun x -> if x |> Array.exists (fun x' -> x' = MenuAction.ExitGame) then gameModeStream.OnNext ExitingGame else ())
