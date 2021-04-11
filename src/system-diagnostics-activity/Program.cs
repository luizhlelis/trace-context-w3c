using System;
using System.Diagnostics;

var upstreamActivity = new Activity("Upstream");

upstreamActivity.Start();
Console.WriteLine("-------------------- {0} --------------------", upstreamActivity.OperationName);
Console.WriteLine("traceparent: {0}", upstreamActivity.Id);
Console.WriteLine("tracestate: {0}", upstreamActivity.TraceStateString);
Console.WriteLine("trace-id: {0}", upstreamActivity.TraceId);
Console.WriteLine("span-id: {0}", upstreamActivity.SpanId);
Console.WriteLine("trace-flags: {0}", upstreamActivity.ActivityTraceFlags);
Console.WriteLine("upstream traceparent: {0}", upstreamActivity.ParentId);
CallChildActivity();
upstreamActivity.Stop();

Console.ReadKey();

void CallChildActivity()
{
    var downstreamActivity = new Activity("Downstream Dependency");

    downstreamActivity.Start();
    Console.WriteLine("-------------------- {0} --------------------", downstreamActivity.OperationName);
    Console.WriteLine("traceparent: {0}", downstreamActivity.Id);
    Console.WriteLine("tracestate: {0}", downstreamActivity.TraceStateString);
    Console.WriteLine("trace-id: {0}", downstreamActivity.TraceId);
    Console.WriteLine("span-id: {0}", downstreamActivity.SpanId);
    Console.WriteLine("trace-flags: {0}", downstreamActivity.ActivityTraceFlags);
    Console.WriteLine("upstream traceparent: {0}", downstreamActivity.ParentId);
    downstreamActivity.Stop();
}