using Grpc.Core;
using Server;

namespace Server.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override async Task GreetManyTimes(GreetingRequest request, IServerStreamWriter<GreetingResponse> responseStream, ServerCallContext context)
    {
        Console.WriteLine("the server received the request : ");
        Console.WriteLine(request.ToString());

        string result = string.Format("hello {0} {1}", request.Greeting.FirstName, request.Greeting.LastName);

        foreach (int i in Enumerable.Range(1, 100))
        {
            await responseStream.WriteAsync(new GreetingResponse() { Result = result });
        }

    }

    public override async Task<GreetingResponse> LongGreet(IAsyncStreamReader<GreetingRequest> requestStream, ServerCallContext context)
    {
        string result = "";

        while (await requestStream.MoveNext())
        {
            result = string.Format("hello {0} {1} {2}",
                requestStream.Current.Greeting.FirstName,
                requestStream.Current.Greeting.LastName,
                Environment.NewLine);
        }

        return new GreetingResponse() { Result = result };
    }

    public override async Task GreetEveryone(IAsyncStreamReader<GreetingRequest> requestStream, IServerStreamWriter<GreetingResponse> responseStream, ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            var result = string.Format("hello {0} {1}",
                requestStream.Current.Greeting.FirstName,
                requestStream.Current.Greeting.LastName);

            Console.WriteLine("received: " + result);
            await responseStream.WriteAsync(new GreetingResponse() { Result = result });
        }
    }
}
