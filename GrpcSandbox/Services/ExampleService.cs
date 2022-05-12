using Grpc.Core;
using GrpcSandbox;

namespace GrpcSandbox.Services
{
    public class ExampleService : ExampleKen.ExampleKenBase
    {
        public override Task<ExampleResponse> UnaryCall(ExampleRequest request, ServerCallContext context)
        {
            //fetch info from requestHeaders
            var userAgent = context.RequestHeaders.GetValue("user-agent");

            var response = new ExampleResponse() { Message = "TestFromKenn"};
            return Task.FromResult(response);
        }

        public override async Task StreamingFromServer(ExampleRequest request, IServerStreamWriter<ExampleResponse> responseStream, ServerCallContext context)
        {
            for (var i = 0; i < 5; i++)
            {
                await responseStream.WriteAsync(new ExampleResponse() { Message = $"message : {i}"});
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            //while (!context.CancellationToken.IsCancellationRequested)
            //{
            //    await responseStream.WriteAsync(new ExampleResponse());
            //    await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
            //}
        }

        public override async Task<ExampleResponse> StreamingFromClient(IAsyncStreamReader<ExampleRequest> requestStream, ServerCallContext context)
        {
            var message = string.Empty;
            while (await requestStream.MoveNext())
            {
                message += requestStream.Current.Name;
                //...
            }

            // OR

            //await foreach (var message in requestStream.ReadAllAsync())
            //{
            //    // ...
            //}

            return new ExampleResponse() { Message = $"TestFromKenn from client : {message}"};
        }

        public override async Task StreamingBothWays(IAsyncStreamReader<ExampleRequest> requestStream, IServerStreamWriter<ExampleResponse> responseStream, ServerCallContext context)
        {
            // Read requests in a background task.
            var readTask = Task.Run(async () =>
            {
                await foreach (var message in requestStream.ReadAllAsync())
                {
                    // Process request.
                    Console.WriteLine(message);
                }
            });

            // Send responses until the client signals that it is complete.
            while (!readTask.IsCompleted)
            {
                await responseStream.WriteAsync(new ExampleResponse() { Message = "TestFromKenn"});
                await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
            }
        }
    }
}
