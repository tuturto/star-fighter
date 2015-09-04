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



let boundingSphereCollision (mob1:Mob) (mob2:Mob) =
    let dx = mob1.location.x - mob2.location.x 
    let dy = mob1.location.y - mob2.location.y
    if dx*dx + dy*dy <= 9216.0f
        then Some ((mob1.location + mob2.location) / 2.0f);
        else None

let pixelPerfectCollision mob1 mob2 =
    None

let collision mob1 mob2 =
    maybe {
            let! crudeCollision = boundingSphereCollision mob1 mob2
            let! preciseCollision = pixelPerfectCollision mob1 mob2
            return preciseCollision
    }
