[Published article](https://dev.to/luizhlelis/using-w3c-trace-context-standard-in-distributed-tracing-3743)

# Using W3C Trace Context standard in distributed tracing

In the software development process, when a system experiences a failure in runtime, it's natural for a developer to try to link that failure with who called that method and also which was the original request. This is where [stack trace](https://en.wikipedia.org/wiki/Stack_trace) comes in, look at an example below:

``` bash
Unhandled exception. System.InvalidOperationException: Stack trace example
   at Program.CallChildActivity() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 22
   at Program.Main() in /Users/luizhlelis/Documents/projects/personal/trace-context-w3c/src/system-diagnostics-activity/Program.cs:line 11
```

with the information above we're able to know that a failure happened at `CallChildActivity()` method (line 22) which was called by the `Main()` method (line 11) both inside the `Program.cs` class. That's the reason why the runtime message tracking is essential for a software health and reliability, a rich information like that greatly increases the troubleshoot productivity. So the `stack trace` suits very well in terms of message trace when the subject is a single process application, like monolithic systems. On the other hand, when dealing with distributed systems like in a microservice architecture, the stack trace is not enough to expose the entire message tracking. That's the reason why distributed tracing tools and standards became necessary. The W3C defines a standard for this type of tracking, which is called `Trace Context`.

## W3C Trace Context purpose

Imagine a system designed as a microservice architecture where two APIs communicate in a synchronous way (http calls) and the second API communicates with a worker by a message broker:

### <a name="firstfigure"></a>Figure 1 - Distributed trace

![distributed-trace](https://raw.githubusercontent.com/luizhlelis/trace-context-w3c/main/doc/distributed-trace.png)

just like the `stack trace`, every `Activity` needs an Id to be identifiable and also needs to know the `Activity` Id of who called it. With the purpose of solving this kind of problem, some vendors came up delivering not only the distributed trace message information but also the application performance, the load time, the application's response time, and other stuff. That kind of vendor is called Application Performance Management tools (APM tools) or also trace systems, below are some examples:

- Dynatrace
- New Relic
- Application Insights
- Elastic APM
- Zipkin

> **_NOTE:_**  I chose to use the term `vendor` to describe all the trace systems because that's the way as the standard refers to them. But it seems that [it'll be changed soon](https://github.com/w3c/trace-context/issues/387).

now imagine a scenery where there are many vendors and also many languages with different diagnostics libraries, some of them identify a trace with an `operation-id`, other calls it as `request-id` and also there is another one which recognizes it as a `trace-id`. Besides that, the id's format changes depending on vendor or diagnostic library: one is in the `hierarchical` format, another one is an `UUID` and there is also a 24 character `string` identifier. That scenery would result in: systems with different tracing vendors will not be able to correlate the traces and also will not be able to propagate traces as there is no unique identification that is forwarded. This is where trace context standard comes in.

## The trace context standard

The [W3C Trace Context](https://www.w3.org/TR/trace-context/) specification defines a standard to HTTP headers and formats to propagate the distributed tracing context information. It defines two fields that should be propagated in the http request's header throughout the trace flow. Take a look below at the standard definition of each field:

- `traceparent`: identifier responsible to describe the incoming request position in its trace graph. It represents a common format of the incoming request in a tracing system, understood by all vendors.

- `tracestate`: extends traceparent with vendor-specific data represented by a set of name/value pairs. Storing information in tracestate is optional.

## The `traceparent` field

The `traceparent` field uses the Augmented Backus-Naur Form (ABNF) notation of [RFC5234](https://www.w3.org/TR/trace-context/#bib-rfc5234) and is composed by 4 sub-fields:

`version` - `traceid` - `parentid/spanid` - `traceflags`

> **_NOTE:_**  `sub-field` term is unofficial, I chose this term for didactic purposes only

for example:

``` bash
00-480e22a2781fe54d992d878662248d94-b4b37b64bb3f6141-00
```

- `version` (8-bit): trace context version that the system has adopted. The current is `00`.

- `trace-id` (16-byte array): the ID of the whole trace. It's used to identify a distributed trace globally through a system.

- `parent-id` / `span-id` (8-byte array): used to identify the parent of the current span on incoming requests or the current span on an outgoing request.

- `trace-flags` (8-bit): flags that represent recommendations of the caller. Can be also thought as the caller recommendations and are strict to three reasons: trust and abuse, bug in the caller or different load between caller and callee service.

> **_NOTE:_** all the fields are encoded as `hexadecimal`

Therefore, applying the trace context concept in an application like the [Figure 1](#firstfigure) will result in the diagram below:

### <a name="secondfigure"></a>Figure 2 - Propagation fields

![propagation-fields](https://raw.githubusercontent.com/luizhlelis/trace-context-w3c/main/doc/w3c-trace-context.png)

note that the `trace-id` is an identifier of all the trace, the `parent-id` identifies a delimited scope of the whole trace. Moreover, the `traceparent` along with the `tracestate` have been propagated throughout the trace flow.

To describe better the `traceparent` dinamic, take a look at the example below, wrote in c#, where two spans scopes are generated and the context is being propagated throughout them:

``` csharp
using System;
using System.Diagnostics;

var upstreamActivity = new Activity("Upstream");

upstreamActivity.Start();
Console.WriteLine(upstreamActivity.OperationName);
Console.WriteLine("traceparent: {0}", upstreamActivity.Id);
CallChildActivity();
upstreamActivity.Stop();

Console.ReadKey();

void CallChildActivity()
{
    var downstreamActivity = new Activity("Downstream Dependency");
    
    downstreamActivity.Start();
    Console.WriteLine(downstreamActivity.OperationName);
    Console.WriteLine("traceparent: {0}", downstreamActivity.Id);
    downstreamActivity.Stop();
}
```

> **_NOTE:_** the `System.Diagnostics.Activity` library in .net 5 has already been configured as the w3c standard

even though the example above shows a single process application, that's the pattern specified by the Trace Context standard. Basically, what that program is doing is: first it opens an upstream span scope and print the `traceparent` in the stdout, then it calls a downstream method which opens another span scope and also prints its `traceparent` and close the scope. After that, the upstream span scope was closed after all of it. The systems output follow below:

```bash
Upstream
traceparent: 00-3e425f2373d89640bde06e8285e7bf88-9a5fdefae3abb440-00

Downstream Dependency
traceparent: 00-3e425f2373d89640bde06e8285e7bf88-0767a6c6c1240f47-00
```

note that the `trace-id` (3e425f2373d89640bde06e8285e7bf88) is been maintained by the whole trace and the `parent-id` is changing based on the span scope (the upstream is equals to 9a5fdefae3abb440 and the downstream is 0767a6c6c1240f47).

In some cases, `parent-id` could cause confusion due to its name, but that name is based on the vision of incoming requests. So one must think in the endpoint side, for example: imagine the message that had just arrived in the controller, the `traceparent` received in the header hasn't had his `parent-id` updated yet by the midleware, so in that vision the id inside `traceparent` is the upstream id or also the id of its parent.

There is a Working Draft (WD) document, the [Trace Context Level 2](https://w3c.github.io/trace-context/), that has an response standard where the `parent-id` calls `child-id`.

> **_NOTE:_** See [w3c process](https://www.w3.org/2017/Process-20170301/#working-draft) for more information about the steps until a document become a w3c recomendation

## The `tracestate` field

The standard uses a fictitious example to describe what is `tracestate` for, I will reproduce it in this article. Imagine a client and server system that use different trace vendors, the first is called Congo and the second is called Rojo. A client traced in the Congo system adds in `tracestate` the vendor-specific id (with its specific format): `tracestate: congo=t61rcWkgMzE`. So the outbound HTTP request will be enriched with the headers below:

```bash
traceparent: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01

tracestate: congo=t61rcWkgMzE
```

any other user-supplied information (different from vendor-specific info) should be added in the [baggage](https://w3c.github.io/baggage/) field, that's another standard which is in Working Draft (WD) step of the [w3c process](https://www.w3.org/2017/Process-20170301/#working-draft) (is not a w3c recomendation yet).

## Trace Context: AMQP protocol

As displayed in [Figure 2](#secondfigure), in a microservice architecture, it's common to propagate messages throw a broker. For that kind of operation, there is another document that specifies the pattern (in case of using AMQP protocol), is the [Trace Context: AMQP protocol](https://w3c.github.io/trace-context-amqp/).

The `Trace Context: AMQP protocol` is another example of document in the Working Draft (WD) step of the [w3c process](https://www.w3.org/2017/Process-20170301/#working-draft). That standard specifies the trace context fields placement in the message different from the HTTP standard.

The standard recomends that the fields `traceparent` and `tracestate` should be added to the message in the `application-properties` section by message publisher. On the message readers side, the trace context should be built by reading `traceparent` and `tracestate` fields from the `message-annotations` first and if not exist, from `application-properties`. See below the message format in the AMQP protocol:

### <a name="thirdfigure"></a>Figure 3 - AMQP message format

![amqp-message-format](https://raw.githubusercontent.com/luizhlelis/trace-context-w3c/main/doc/amqp-message-format.png)

The reason for the trace context fields placement in the message is that the `application-properties` section is defined by the message publisher and the brokers cannot mutate those properties because that section is immutable. On the other hand, the section `message-annotations` is designed for message brokers usage. In other words, the fields inside that section can be mutated during the message processing. So it means that in case the need arises to annotate the message inside the middleware as it flows, that must happen in the `message-annotations` section, using the fields sent by the publisher in `application-properties` as a base.

## Conclusion

The W3C Trace Context standard came to define a pattern to the distributed tracing process. Currently, there is only one `W3C Recommendation` which is for HTTP calls (lauched in february 2020), all the other standards are in working in process (AMQP, MQTT and baggage). It doesn't means that you should avoid to use the standard in a production environment, but keep in mind that some things are going to change and it's important to be up to date with newer releases.

If you got until here and liked the article content, let me know reacting to the current post. You can also open a discussion below, I'll try to answer soon. On the other hand, if you think that I said something wrong, please open an issue in the [article's github repo](https://github.com/luizhlelis/trace-context-w3c). In the next article, I'll show a full distributed trace example using the trace context concept (in a microsservice architecture using `.NET 5`). Hope you like it!

## References

BOGARD, Jimmy. [Building End-to-End Diagnostics and Tracing: Trace Context](https://jimmybogard.com/building-end-to-end-diagnostics-and-tracing-a-primer-trace-context/)

DRUTU, Bogdan; KANZHELEV, Sergey; MCLEAN, Morgan; MOLNAR, Nik; REITBAUER, Alois; SHKURO, Yuri. [W3C Recommendation - Trace Context](https://www.w3.org/TR/trace-context/)

DRUTU, Bogdan; KANZHELEV, Sergey; MCLEAN, Morgan; MOLNAR, Nik; REITBAUER, Alois; SHKURO, Yuri. [W3C Recommendation - Trace Context Level 2](https://w3c.github.io/trace-context/)

GODFREY, Robert; INGHAM, David; SCHLOMING, Rafael. [Advanced Message Queuing Protocol (AMQP) Version 1.0, Part 3: Messaging](http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-messaging-v1.0-os.html)

KANZHELEV, Sergey; MCLEAN, Morgan, REITBAUER, Alois. [W3C Editor's draft - Propagation format for distributed trace context: Baggage](https://w3c.github.io/baggage/)

KANZHELEV, Sergey; VASTERS, Clemens. [W3C Editor's draft - Trace Context: AMQP protocol](https://w3c.github.io/trace-context-amqp/)

KANZHELEV, Sergey; VASTERS, Clemens. [W3C Editor's draft - Trace Context: MQTT protocol](https://w3c.github.io/trace-context-mqtt/)

NEVILE, Charles M. [World Wide Web Consortium Process Document](https://www.w3.org/2017/Process-20170301/)
