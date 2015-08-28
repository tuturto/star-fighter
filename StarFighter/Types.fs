module Types

open System.Reactive.Subjects 

type GameMode =
    | Menu

/// Current mode of the game
let gameModeStream = new BehaviorSubject<GameMode>(Menu)
