module Types

open System.Reactive.Subjects 
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open RxNA.Renderer

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
             texture: Texture }

type BulletCollisionInfo = 
     | EnemyCollision of enemy : Mob * bullet : Mob * time : GameTime
     | NoCollision of bullet : Mob * time : GameTime
     member this.Collided =
        match this with
            | EnemyCollision _ -> true
            | NoCollision _ -> false
     member this.Bullet =
        match this with
            | EnemyCollision (_, bullet, _) -> bullet
            | NoCollision (bullet, _) -> bullet
     member this.Enemy =
        match this with
            | EnemyCollision (enemy, _, _) -> Some enemy
            | NoCollision _ -> None
     member this.Time =
        match this with
            | EnemyCollision (_, _, time) -> time
            | NoCollision (_, time) -> time

let collisionToMob res time (info:BulletCollisionInfo) =
    let enemy = info.Enemy.Value 
    { location = enemy.location;
      speed = enemy.speed;
      texture = convert time <| res.textures.Item "large explosion" }

let R = System.Random()

/// Current mode of the game
let gameModeStream = new BehaviorSubject<GameMode>(Menu)

let enemyBulletCollisions = 
    new System.Reactive.Subjects.BehaviorSubject<BulletCollisionInfo list>([])
