module Types

open System.Reactive.Subjects 
open Microsoft.Xna.Framework.Graphics

type GameMode =
    | Menu
    | ExitingGame

type Mob = { x: float32;
             y: float32;
             dx: float32;
             dy: float32;
             texture: Texture2D; }

let R = System.Random()

/// Current mode of the game
let gameModeStream = new BehaviorSubject<GameMode>(Menu)
