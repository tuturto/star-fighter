module Sounds

open Microsoft.Xna.Framework.Audio 

open RxNA.Renderer 
open Types

let soundUpdater (res:RenderResources) (event:SoundEvent) =
    match event with 
        | Silence -> false
        | MachinegunFired -> (res.sounds.Item "machinegun").Play()
        | ShotgunFired -> (res.sounds.Item "shotgun").Play()
        | DualshotFired -> (res.sounds.Item "dualshot").Play()
        | BulletImpact -> false
        | Explosion -> (res.sounds.Item "explosion").Play()
        | PowerUpCollected -> (res.sounds.Item "power up").Play()
    |> ignore
