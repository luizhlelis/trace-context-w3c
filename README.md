# W3C Trace Context

Here you're gonna find a simple description of what is the purpose of [W3C Trace Context](https://www.w3.org/TR/trace-context/) and what kind of problem it came to solve.

## Simple Stack Trace

``` bash
Unhandled exception. System.InvalidOperationException: Stack trace example
   at Program.CallChildActivity() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 22
   at Program.Main() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 11
```

## Propagation fields

1 - `traceparent`: indentifier responsable to describe the incoming request position in its trace graph;

2 -  `tracestate`: extends traceparent with vendor-specific data represented by a set of name/value pairs. Storing information in tracestate is optional.

## Traceparent Header

1 - `version`
2 - `trace-id`
3 - `parent-id`
4 - `trace-flags`
