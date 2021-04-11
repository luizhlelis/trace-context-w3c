using System;
using System.Diagnostics;

var parentActivity = new Activity("Parent");

parentActivity.Start();
Console.WriteLine("{0} , id - {1}", parentActivity.OperationName, parentActivity.Id);
CallChildActivity();
parentActivity.Stop();

Console.ReadKey();

void CallChildActivity()
{
    var childActivity = new Activity("Child");

    childActivity.Start();
    Console.WriteLine("{0} , id - {1}", childActivity.OperationName, childActivity.Id);
    childActivity.Stop();
}