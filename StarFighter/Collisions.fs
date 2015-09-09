module Collisions

open ExtCore.Control.WorkflowBuilders

open Types


let boundingSphereCollision (mob1:Mob) (mob2:Mob) =
    let dx = mob1.location.x - mob2.location.x 
    let dy = mob1.location.y - mob2.location.y
    let width = (float32)(mob1.texture.Width + mob2.texture.Width) / 2.0f
    let distance = width * width
    if dx*dx + dy*dy <= (float32)distance
        then Some ((mob1.location + mob2.location) / 2.0f);
        else None

let pixelPerfectCollision mob1 mob2 =
    None

let collision mob1 mob2 =
    maybe {
            return! boundingSphereCollision mob1 mob2
    }
