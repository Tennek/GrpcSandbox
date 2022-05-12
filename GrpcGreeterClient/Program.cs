using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcGreeterClient;

#region *** Unary Call
// The port number must match the port of the gRPC server.
//using var channel = GrpcChannel.ForAddress("https://localhost:7277");
//var client = new Greeter.GreeterClient(channel);
//var reply = await client.SayHello2Async(
//                  new HelloRequest { Name = "GreeterClient" });
//Console.WriteLine("Greeting: " + reply.Message);
#endregion

#region *** Streaming from server
//using var channel = GrpcChannel.ForAddress("https://localhost:7277");
//var client = new ExampleKen.ExampleKenClient(channel);
//using var call = client.StreamingFromServer(new ExampleRequest() { Name = "Kenneth" });
//while (await call.ResponseStream.MoveNext(new CancellationToken()))
//{
//    Console.WriteLine("Greeting: " + call.ResponseStream.Current.Message);
//    // "Greeting: Hello World" is written multiple times
//}
#endregion

#region *** Streaming from client
//using var channel = GrpcChannel.ForAddress("https://localhost:7277");
//var client = new ExampleKen.ExampleKenClient(channel);
//using var call = client.StreamingFromClient();
//for (var i = 0; i < 3; i++)
//{
//    await call.RequestStream.WriteAsync(new ExampleRequest { Name = i.ToString() });
//}
//await call.RequestStream.CompleteAsync();
//var response = await call;
//Console.WriteLine($"Count: {response.Message}");
#endregion

#region *** Streaming both ways
using var channel = GrpcChannel.ForAddress("https://localhost:7277");
var client = new ExampleKen.ExampleKenClient(channel);
using var call = client.StreamingBothWays();
Console.WriteLine("Starting background task to receive messages");
var readTask = Task.Run(async () =>
{
    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response.Message);
        // Echo messages sent to the service
    }
});
Console.WriteLine("Starting to send messages");
Console.WriteLine("Type a message to echo then press enter.");

var messageCounter = 0;
while (messageCounter < 3) //send 3 messages then quit
{
    var result = Console.ReadLine();
    if (string.IsNullOrEmpty(result))
    {
        break;
    }
    messageCounter++;
    await call.RequestStream.WriteAsync(new ExampleRequest { Name = result });
}

Console.WriteLine("Disconnecting");
await call.RequestStream.CompleteAsync();
await readTask;
#endregion


Console.WriteLine("Press any key to exit...");
Console.ReadKey();