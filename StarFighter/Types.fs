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

type BulletCollisionInfo = 
     | EnemyCollision of enemy : Mob * bullet : Mob
     | NoCollision of bullet : Mob
     member this.Collided =
        match this with
            | EnemyCollision (_, _) -> true
            | NoCollision _ -> false
     member this.Bullet =
        match this with
            | EnemyCollision (_, bullet) -> bullet
            | NoCollision bullet -> bullet
     member this.Enemy =
        match this with
            | EnemyCollision (enemy, _) -> Some enemy
            | NoCollision _ -> None

let R = System.Random()

/// Current mode of the game
let gameModeStream = new BehaviorSubject<GameMode>(Menu)

let enemyBulletCollisions = 
    new System.Reactive.Subjects.BehaviorSubject<BulletCollisionInfo list>([])
