module Collisions

open Types

let boundingSphereCollision (mob1:Mob) (mob2:Mob) =
    let dx = mob1.location.x - mob2.location.x 
    let dy = mob1.location.y - mob2.location.y
    if dx*dx + dy*dy <= 9216.0f
        then 
            System.Diagnostics.Debug.WriteLine [mob1.location.x; mob1.location.y]
            System.Diagnostics.Debug.WriteLine [mob2.location.x; mob2.location.y]
            System.Diagnostics.Debug.WriteLine [dx; dy]
            Some ((mob1.location + mob2.location) / 2.0f);
        else None

let pixelPerfectCollision mob1 mob2 =
    None

let collision mob1 mob2 =
    maybe {
            return! boundingSphereCollision mob1 mob2
    }
