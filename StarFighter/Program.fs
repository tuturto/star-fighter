open Game

[<EntryPoint>]
let main argv = 
    use g = new Game()
    g.Window.Title <- "Star Fighter"
    g.Run()
    0 // return an integer exit code
