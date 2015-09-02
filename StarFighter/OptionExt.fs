module Option

let filter cond =
    function
    | Some a as s when cond a -> s
    | _ -> None

let flatten =
    function
    | Some a -> a
    | None -> None

let getOrElse d = 
    function
    | Some a -> a
    | None -> d

let orElse b a =
    match a with
    | Some _ -> a
    | None -> b

let lift2 f x y =
    match x with 
    | None -> None
    | Some x' -> 
        match y with
        | None -> None
        | Some y' -> Some <| f x' y'

let lift3 f x y z = 
    match x with 
    | None -> None
    | Some x' -> 
        match y with
        | None -> None
        | Some y' -> 
            match z with
            | None -> None
            | Some z' -> Some <| f x' y' z'
