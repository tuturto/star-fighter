module Collisions

open ExtCore.Control.WorkflowBuilders
open RxNA.Renderer

open Types


let boundingSphereCollision gameTime (mob1:Mob) (mob2:Mob) =
    let dx = mob1.location.x - mob2.location.x 
    let dy = mob1.location.y - mob2.location.y
    let texture1 = currentFrame gameTime mob1.texture 
    let texture2 = currentFrame gameTime mob2.texture 
    let width = (float32)(texture1.Width + texture2.Width) / 2.0f
    let distance = width * width
    if dx*dx + dy*dy <= (float32)distance
        then Some ((mob1.location + mob2.location) / 2.0f);
        else None

let pixelPerfectCollision mob1 mob2 =
    None

let collision gameTime mob1 mob2 =
    maybe {
            return! boundingSphereCollision gameTime mob1 mob2
    }
