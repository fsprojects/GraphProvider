// Originally ported from http://fsharp3sample.codeplex.com
module GraphProvider.StateMachine

open System.Xml
open System.Xml.Linq

type IState = 
    abstract member EnterFunction : unit -> unit
    abstract member ExitFunction : unit->unit

// define a node record
type Node = { Name : string; NextNodes : (string option * Node) list}

// define DGML DU
type DGML = 
    | Node of string
    | Link of string * string * string option


// define DGML class
type DGMLClass() = class   
    let mutable nodes = Unchecked.defaultof<Node list>
    let mutable links = Unchecked.defaultof<DGML list>    
    let mutable currentState = System.String.Empty

    // current state
    member this.CurrentState 
        with get() = currentState
        and private set(v) = currentState <- v

    // all links in the DGML file
    member this.Links 
        with get() = links
        and private set(v) = links <- v

    // all nodes in the DGML file
    member this.Nodes 
        with get() = nodes
        and private set(v) = nodes <- v              

    // initialize the state machien from fileName and set the initial state to initState
    member this.Init(fileName:string, initState) = 
        let file = XDocument.Load(fileName, LoadOptions.None)
        this.Links <- 
            file.Descendants() 
            |> Seq.filter (fun node -> node.Name.LocalName = "Link")
            |> Seq.map (fun node -> 
                            let sourceName = node.Attribute(XName.Get("Source")).Value
                            let targetName = node.Attribute(XName.Get("Target")).Value                            
                            let label = 
                              let attr = node.Attribute(XName.Get("Label"))
                              if attr = null then None else Some(attr.Value)
                            DGML.Link(sourceName, targetName,label))
            |> Seq.toList

        let getNextNodes fromNodeName= 
                this.Links 
                |> Seq.choose (function 
                    | Link(a, b, l) when a = fromNodeName ->
                            match this.FindNode(b) with 
                            | Some(x) -> Some(l,x) 
                            | None -> None
                    | _ -> None)
                |> Seq.toList

        this.Nodes <-
            file.Descendants() 
            |> Seq.filter (fun node -> node.Name.LocalName = "Node")
            |> Seq.map (fun node -> node.Attribute(XName.Get("Id")).Value )
            |> Seq.map (fun n -> { Node.Name=n; NextNodes = [] })
            |> Seq.toList
        
        this.Nodes <-    
            this.Nodes
            |> Seq.map (fun n -> { n with NextNodes = (getNextNodes n.Name) } )  
            |> Seq.toList

        this.CurrentState <- initState

    // fine the node by given nodeName
    member this.FindNode(nodeName) : Node option= 
        let result = 
            this.Nodes 
            |> Seq.filter (fun n -> n.Name = nodeName)
        if result |> Seq.isEmpty then None
        else result |> Seq.head |> Some    

    // current node
    member this.CurrentNode
        with get() = 
            this.Nodes 
            |> Seq.filter (fun n -> n.Name = this.CurrentState)
            |> Seq.head
            
    // determine if can transit to a node represented by the nodeName
    member this.CanTransitTo(nodeName:string) =
        this.CurrentNode.NextNodes |> Seq.exists (fun (_,n) -> n.Name = nodeName)

    // force current state to a new state
    member this.ForceStateTo(args) = 
        this.CurrentState <- args        
end

// state machine class which inherit the DGML class
// and use a MailboxProcessor to perform asynchronous message processing (if makeAsync = true)
type StateMachine(makeAsync) as this = 
    inherit DGMLClass()

    let functions = System.Collections.Generic.Dictionary<string, IState>()
    let transit newState =
        if this.CanTransitTo newState then
          this.InvokeExit(this.CurrentNode.Name)
          this.ForceStateTo newState
          this.InvokeEnter newState

    let processor = new MailboxProcessor<string>(fun inbox ->
                    let rec loop () = 
                        async {
                            let! newState = inbox.Receive()
                            transit newState
                            return! loop ()
                        }
                    loop ())    

    do 
        if makeAsync then processor.Start()

    // define the second constructor taking the file name and initial state name
    new(fileName, makeAsync, initState) as secondCtor = 
        new StateMachine(makeAsync)
        then
            secondCtor.Init(fileName, initState)

    // asynchronously or synchronously transit to a new state
    member this.TransitTo(state) = 
        if makeAsync then processor.Post state else transit state

    // FindPath - Slow version
    member this.FindShortestPathTo(startNode,targetNode) = 
        let findIndex name = this.Nodes |> Seq.findIndex (fun x -> x.Name = name)

        let dist = 
            this.Nodes
            |> Seq.map (fun x -> (System.Int32.MaxValue, x))
            |> Array.ofSeq

        let prev = Array.create (this.Nodes.Length) Option<Node>.None
        let startIndex = findIndex startNode
        dist.[startIndex] <- (0, dist.[startIndex] |> snd)

        let rec processNode visitedNodes = 
            let (act, u) = 
                dist
                |> Seq.filter (fun x -> 
                       visitedNodes
                       |> Seq.tryFind ((=) (snd x))
                       |> Option.isNone)
                |> Seq.minBy fst
            if u.Name = targetNode then 
                let rec path (node : Node) = 
                    let index = findIndex node.Name
                    match prev.[index] with
                    | Some n -> n.Name :: path n
                    | None -> []
            
                match path u with
                | [] -> None
                | x -> Some x
            else 
                let uIndex = findIndex u.Name
                let neighbors = u.NextNodes |> Seq.map snd
                let alt = act + 1
                for n in neighbors do
                    let nIndex = findIndex n.Name
                    let curDist = dist.[nIndex] |> fst
                    if alt < curDist then 
                        dist.[nIndex] <- (alt, dist.[nIndex] |> snd)
                        prev.[nIndex] <- Some this.Nodes.[uIndex]
                processNode (u :: visitedNodes)

        processNode []

    // set the transition function
    member this.SetFunction(name:string, state:IState) = 
        if functions.ContainsKey(name) then 
            functions.[name] <- state
        else
            functions.Add(name, state)

    // invoke the Exit function
    member private this.InvokeExit(name:string) = 
        if functions.ContainsKey(name) then
            functions.[name].ExitFunction()

    // invoke the Enter function
    member private this.InvokeEnter(name:string) = 
        if functions.ContainsKey(name) then
            functions.[name].EnterFunction()

type State = { Name:string }