# W3C Trace Context

Here you're gonna find a simple description of what is the purpose of [W3C Trace Context](https://www.w3.org/TR/trace-context/) and what kind of problem it came to solve.

## Simple Stack Trace

``` bash
Unhandled exception. System.InvalidOperationException: Stack trace example
   at Program.CallChildActivity() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 22
   at Program.Main() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 11
```

![Distributed Trace](doc/distributed-trace.png)

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
