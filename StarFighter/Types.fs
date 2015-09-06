module Types

open System.Reactive.Subjects 
open Microsoft.Xna.Framework.Graphics

type GameMode =
    | Menu
    | GameRunning
    | ExitingGame

type Speed = 
     { dx: float32
       dy: float32 }
     static member (*) (speed, scalar) =
        { dx = speed.dx * scalar;
          dy = speed.dy * scalar; }

type Location = 
     { x: float32
       y: float32 }
     static member (+) (location:Location, speed:Speed) =
        { x = location.x + speed.dx;
          y = location.y + speed.dy; }
     static member (+) (location1:Location, location2:Location) =
        { x = location1.x + location2.x;
          y = location1.y + location2.y; }
     static member (/) (location, scalar) =
        { x = location.x / scalar;
          y = location.y / scalar; }

type Mob = { location: Location
             speed: Speed
             texture: Texture2D }

let R = System.Random()

/// Current mode of the game
let gameModeStream = new BehaviorSubject<GameMode>(Menu)

type MaybeBuilder() =

    member this.Bind(x, f) = 
        match x with
        | None -> None
        | Some a -> f a

    member this.Return(x) = 
        Some x

    member this.ReturnFrom(m) = 
        m
   
let maybe = new MaybeBuilder()
