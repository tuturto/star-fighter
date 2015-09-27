module Collisions

open ExtCore.Control.WorkflowBuilders
open RxNA.Renderer

open Types


let inline boundingSphereCollision gameTime (mob1:^a) (mob2:^b) =
    let location1 = (^a : (member Location : Location) mob1)
    let location2 = (^b : (member Location : Location) mob2)
    let texture1 = currentFrame gameTime (^a : (member Texture : Texture) mob1)
    let texture2 = currentFrame gameTime (^b : (member Texture : Texture) mob2)
    let dx = location1.x - location2.x 
    let dy = location1.y - location2.y
    let width = (float32)(texture1.Width + texture2.Width) / 2.0f
    let distance = width * width
    if dx*dx + dy*dy <= (float32)distance
        then Some ((location1+ location2) / 2.0f);
        else None

let inline pixelPerfectCollision mob1 mob2 =
    None

let inline collision gameTime mob1 mob2 =
    let location2 = (^b : (member Location : Location) mob2)
    let texture2 = currentFrame gameTime (^b : (member Texture : Texture) mob2)
    maybe {
            return! boundingSphereCollision gameTime mob1 mob2
    }
