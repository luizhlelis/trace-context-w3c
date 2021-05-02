# W3C's Trace Context standard and how to enrich message tracing with APM tools

In the software development process, when a system experiences a failure in runtime, it's natural for a developer to try to link that failure with who called that method and also which was the original request. This is where [stack trace](https://en.wikipedia.org/wiki/Stack_trace) comes in, look at an example below:

``` bash
Unhandled exception. System.InvalidOperationException: Stack trace example
   at Program.CallChildActivity() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 22
   at Program.Main() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 11
```

with the information above we're able to know that a failure happened at `CallChildActivity()` method (line 22) which was called by the `Main()` method (line 11) both inside the `Program.cs` class. That's the reason why the runtime message tracking is essential for a software maintenance and support, a rich information like that greatly increases the troubleshoot productivity. So the `stack trace` serves very well in terms of message trace when the subject is a single process application, like monolithic systems. On the other hand, when dealing with distributed systems like in a microservice architecture, the stack trace is not enough to expose the entire message tracking, that's the reason why distributed tracing tools and standards became necessary. The W3C defines a standard for this type of tracking, which is called `Trace Context`.

## W3C Trace Context purpose

Imagine a system designed as a microservice architecture where two APIs communicate in a synchronous way (http calls) and the second API communicates with a worker by a message broker:

**Figure 1** - Distributed trace

![distributed-trace](doc/distributed-trace.png)

just like the `stack trace`, every `Activity` needs an Id to be identifiable and also needs to know the `Activity` Id of who called it. With the purpose of solve this kind of problem, some vendors came up delivering not only the distributed trace message information but also the application performance, the load time, the application's response time, and other stuff. That kind of vendor are called Application Performance Management tools (APM tools), below are some examples:

- Dynatrace
- New Relic
- Application Insights
- Elastic APM
- Zipkin

now imagine a scenery where there are many APM vendors and also many languages with different diagnostics libraries, some of them identify a trace with an `operation-id`, other calls it as `request-id` and also there is another one which reconize it as a `trace-id`, besides that, the id's format changes depending on vendor or diagnostic library: one is in the `hierarchical` format, another one is an `UUID` and there is also a 24 character `string` identifier. This is where [trace context](https://www.w3.org/TR/trace-context/) standard comes in.

## The trace context standard

Here you're gonna find a simple description of what is the purpose of [W3C Trace Context](https://www.w3.org/TR/trace-context/) and what kind of problem it came to solve.

Defines standard HTTP headers and a value format to propagate context information that enables distributed tracing scenarios.

![Distributed Trace](doc/w3c-trace-context.png)

## Propagation fields

- `traceparent`: indentifier responsable to describe the incoming request position in its trace graph;

- `tracestate`: extends traceparent with vendor-specific data represented by a set of name/value pairs. Storing information in tracestate is optional.

## Traceparent Header

This section uses the Augmented Backus-Naur Form (ABNF) notation of [RFC5234](https://www.w3.org/TR/trace-context/#bib-rfc5234).

Format:

`version` - `traceid` - `parentid/spanid` - `traceflags`

Example:

``` bash
00-480e22a2781fe54d992d878662248d94-b4b37b64bb3f6141-00
```

- `version` (8-bit): trace context version that the system has adopted. The current is `00`.

- `trace-id` (16-byte array): the ID of the whole trace and is used to identify a distributed trace globally through a system.

- `parent-id` / `span-id` (8-byte array): used to identify the parent of the current span on incoming requests or the current span on an outgoing request.

- `trace-flags` (8-bit): flags that represent recommendations of the caller: trust and abuse, bug in the caller or different load between caller and callee service might force callee to downsample.

Ps: all the fields are represented as [hexadecimal](https://www.cs.princeton.edu/courses/archive/fall07/cos109/bc.html) not binary.

take a look at [sample code](src/).
