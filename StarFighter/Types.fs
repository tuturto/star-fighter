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
           member this.Location = this.location     
           member this.Texture = this.texture 

type Enemy = { location: Location
               speed: Speed
               texture: Texture
               hp: int 
               score: int }
             member this.Location = this.location     
             member this.Texture = this.texture 

type ExplodingPlayerInfo = { location: Location
                             speed: Speed 
                             explosionTime: float }
                           member this.Location = this.location
                           member this.Texture = this.Texture

type Player =
     | NormalPlayer of Mob
     | ExplodingPlayer of ExplodingPlayerInfo

type PowerUpType =
    | Machinegun
    | Shotgun
    | Dualshot

type PowerUp = { location: Location
                 speed: Speed
                 texture: Texture
                 weapon: PowerUpType }
               member this.Location = this.location     
               member this.Texture = this.texture 

type SoundEvent =
     | Silence
     | MachinegunFired
     | ShotgunFired
     | DualshotFired
     | BulletImpact
     | Explosion
     | PowerUpCollected

type BulletInfo = { fired: float
                    weapon: PowerUpType
                    bullets: Mob list }

type BulletCollisionInfo = 
     | EnemyCollision of enemy : Enemy * bullet : Mob * location : Location * time : GameTime
     | NoCollision of bullet : Mob * time : GameTime
     member this.Collided =
        match this with
            | EnemyCollision _ -> true
            | NoCollision _ -> false
     member this.Bullet =
        match this with
            | EnemyCollision (_, bullet, _, _) -> bullet
            | NoCollision (bullet, _) -> bullet
     member this.Enemy =
        match this with
            | EnemyCollision (enemy, _, _, _) -> Some enemy
            | NoCollision _ -> None
     member this.Time =
        match this with
            | EnemyCollision (_, _, _, time) -> time
            | NoCollision (_, time) -> time
     member this.Location =
        match this with
            | EnemyCollision (_, _, location, _) -> Some location
            | NoCollision _ -> None

/// Current mode of the game
let gameModeStream = new BehaviorSubject<GameMode>(Menu)
