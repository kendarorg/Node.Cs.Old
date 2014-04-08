Node.Cs Node.Js Coroutine C# Web Kendar.org


Documentation on http://www.kendar.org/?p=/dotnet/nodecs

This is a project developed to simulate the behaviour of Node.Js. It's not a direct port since simply I prefer
the approach proposed here :) . Nothing against Node.Js developers, i love their work!

The idea is to leverage on the usage of "Coroutines" with the simple implementation offered by the C# IEnumerable
and IEnumerator. I could have made something based on the new await and async, but I like to loose myself in the
darkest and deepest forrest, for the sake of knowledge... or masochism sometimes.

* 5000 requests per second with dynamic content with a single thread (2 static 1Kb pages, 2 dynamic 1Kb pages, 1 static 3Kb image, JMeter with 100 concurrent threads).
* Single threaded operations, no need for synchronization primitives
* MVC Architecture based on Controllers and Cshtml views (thanks RazorEngine!)
* No reflection used by my code, all is made through runtime generated expressions (thanks ExpressionBuilder and ClassWrapper)
* No locks, heavy usage of CAS atomic operations
* All long running operations for the server can be asynchronous
* Responsive until 5k request per second (at least, with the load specified at the first point)
* Around 8Mb/s throughput with single core and 100Mbit network (at least, with the load specified at the first point)
* Clean and easily debuggable execution flow
* Based on "unit of works" easy to debug in isolation
* Predisposition for IOC containers for the creation of controllers
* Multiple MERGEABLE sources for files and DLL. It would be possible for example to use the whole production environment overriding locally only the changed parts and seeing the two as one whole thing.
* Cookies management
* In memory session management
* Basic Authentication with custom providers
* Automatic Server Recycle
