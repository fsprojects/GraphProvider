module GraphProvider.TypeProviders.Graph.Tests.FindPathToTests

open NUnit.Framework
open GraphProvider
open GraphProvider.TypeProviders

type Actors = Graph<"Actors.dgml">

[<Test>]
let ``Can find path between Keanu Reeves and Kevin Bacon``() =   
    let path = Actors.StartFromKeanuReeves().ShortestPathToKevinBacon()
    Assert.AreEqual(["The Matrix"; "Laurence Fishburne"; "Mystic River"],path)