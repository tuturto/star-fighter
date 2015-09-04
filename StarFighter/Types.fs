module Types

open System.Reactive.Subjects 
open Microsoft.Xna.Framework.Graphics

type GameMode =
    | Menu
    | GameRunning
    | ExitingGame

type Location = { x: float32
                  y: float32 }

type Mob = { location: Location
             dx: float32
             dy: float32
             texture: Texture2D }

let R = System.Random()

/// Current mode of the game
let gameModeStream = new BehaviorSubject<GameMode>(Menu)
