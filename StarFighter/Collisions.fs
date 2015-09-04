module Collisions

open Types

type MaybeBuilder() =

    member this.Bind(x, f) = 
        match x with
        | None -> None
        | Some a -> f a

    member this.Return(x) = 
        Some x
   
let maybe = new MaybeBuilder()



let boundingSphereCollision mob1 mob2 =
    let dx = mob1.x - mob2.x 
    let dy = mob1.y - mob2.y
    if dx*dx + dy*dy <= 9216.0f
        then Some { Location.x = (mob1.x + mob2.x) / 2.0f;
                    Location.y = (mob1.y + mob2.y) / 2.0f; }
        else None

let pixelPerfectCollision mob1 mob2 =
    None

let collision mob1 mob2 =
    maybe {
            let! crudeCollision = boundingSphereCollision mob1 mob2
            let! preciseCollision = pixelPerfectCollision mob1 mob2
            return preciseCollision
    }
