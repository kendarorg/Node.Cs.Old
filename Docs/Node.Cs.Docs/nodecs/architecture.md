<!--settings(
title=Node.Cs Architecture
description=Node.Cs Architecture.
)-->

## {Title}

The whole Node.Cs architecture is heavily based on the concpets already defined for Node.js.

### Introduction

Let's start explaining the Node (.js) approach.

Think about several people trying to order their food in a fast food restaurant, a common situation here in Italy
is to see a bunch of guys trying to reach the desk to place the order without any idea of what a queue is.

The result is that the waiters will beccome mad while figuring out who will be the next in line and taking the orders.
With this kind of approach there will be a lot of time used only to manage the contention of the "desk resource".
Some customer will receive its meal in a couple of minutes whilst some other will wait for a longer time. The
weakest will not receive their meals at all, since they will not be able to reach the desk or to place the order.

If there are, instead, several well ordered queues, the first arrived will be the first served,and while everybody
will have to wait a bit longer, all will receive their meals in a fair way.
With this kind of approach the real "computation time" will be used to take the order and not wasted in ancillary
activities like kicking the most troublesome people out of the shop! And all the people, while waiting a little longer,
will be served!!

### The .NET async/await approach

![Thread pool approach]({This}multithread.png)

A lot of ink had been spent explaining the new async and await keywords added to the .Net framework to ease the
developement of performance wise application. The concept is to hide all the complexity of the asynchronous operations
with some "synctactic sugar". 

What happens under the hood? Calling an async operation means putting a task to execute inside the thread pool and block
the caller task/thread until its completion. This is totally correct when dealing with heavily CPU-based operations.
The programmer will block the caller task, leaving the CPU free and move the processing power to the callee. 

__BUT__

We are continuing to deal with interruptions, with synchronizations and we are still using processing power to
manage the whole thing. If i have to deal with writing a file, for example, i tell the disk controller to read certain
bytes from the disk itself and... I WAIT until the momenti in wich the disk gives me an answer filling my buffers
with the data. I don't need (in theory) to run anything on the caller! Whilist having a couple of threads blocked!

__AND__

The resources contention is still a problem. There could be easily two tasks trying to access the same resource!

### The Node.Js (and Node.Cs) approach

![Event driven approach]({This}eventdriven.png)

With Node.Js and Node.Cs the caller task is putted in an inactive state, but the thread in which it is running is used 
to execute other operations. Other than this it is possible to queue requests. This means that if i have concurrent access
to an heavily concurrent resource, think about a data cache between the business layer and the database, only one request at 
time will be execute and there will be no overhead for locking or syncronization.

Practically this is obtained with a single thread, on wich all operations are queued. Then they are executed -one after the other-
there will not exist a "concurrent" operation.

Think about a web server:

A request arrive to Node. Than an asyncronous task is invoked to read the data from the network interfaces. We use a task
because this is a heavily I/O bound operation. In the meantime a new operation is queued on Node that is waiting the 
completion of the "receive" task.
When the data had been read, the operation is executed. Suppose that was a request to a static file. The operation invoke
a "cache" operation requesting the object from cache, and leaves control to the other operations "waiting". 
The cache operation reads the data from a simple dictionary (it's the only thing executing when it's running) get the data 
return the control to the caller. When the caller will be executed another time it will be restarted and will have
the data.

In all this cycle we used a single thread and an asynchronous task. We did'nt used any sinchronization primitive and
nothing needed to be thread safe.

### Credits

Images taken by http://www.yesodweb.com/blog/2012/11/warp-posa under [Creative Commons](http://creativecommons.org/licenses/by/4.0/) licence.