(*** hide ***)
#I "../../bin"

(**
GraphProvider
===========================

The GraphProvider project implements a state machine type provider..

* [CommonFolders](CommonFolders.html)

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The library can be <a href="https://nuget.org/packages/GraphProvider">installed from NuGet</a>:
      <pre>PM> Install-Package GraphProvider</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Example
-------

This example demonstrates the use of the GraphProvider:

![alt text](img/Turnstile.png "Turnstile")

*)
// reference the type provider dll
#r "GraphProvider.dll"
open GraphProvider

// Let the type provider do it's work
type Turnstile = Graph<"Turnstile.dgml"> 

(**

Now we have full intellisense for the DGML file:
![alt text](img/Intellisense.png "Intellisense for DGML files")

*)

// Turnstile is locked initially
Turnstile.StartFromLocked()
// [fsi:val it : GraphProvider.Graph<...>.Locked2 = {Name = "Locked";}]

// Insert a coin and the turnstile is unlocked
Turnstile.StartFromLocked().Coin()
// [fsi:val it : Graph<...>.Unlocked2 = {Name = "Unlocked";}]
(**

Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/fsprojects/GraphProvider/tree/master/docs/content
  [gh]: https://github.com/fsprojects/GraphProvider
  [issues]: https://github.com/fsprojects/GraphProvider/issues
  [readme]: https://github.com/fsprojects/GraphProvider/blob/master/README.md
  [license]: https://github.com/fsprojects/GraphProvider/blob/master/LICENSE.txt
*)