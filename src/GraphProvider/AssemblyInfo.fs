namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("GraphProvider")>]
[<assembly: AssemblyProductAttribute("GraphProvider")>]
[<assembly: AssemblyDescriptionAttribute("This library is for the .NET platform implementing a state machine type provider.")>]
[<assembly: AssemblyVersionAttribute("0.0.1")>]
[<assembly: AssemblyFileVersionAttribute("0.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
