using Grpc.Net.Client;
using Server;

var channel = GrpcChannel.ForAddress("https://localhost:7277");
var client = new Greeter.GreeterClient(channel);

var greeting = new Greeting
{
    FirstName = "Jareb",
    LastName = "Smith",
};

//await UrinaryGreet(client);

var request = new GreetingRequest()
{
    Greeting = greeting,
};

//await ServerStreamGreet(client, request);

//await ClientStreamGreet(client, request);

await BiDirStreamGreet(client);


channel.ShutdownAsync().Wait();
Console.ReadKey();

static async Task BiDirStreamGreet(Greeter.GreeterClient client)
{
    var stream = client.GreetEveryone();

    var responseReaderTask = Task.Run(async () =>
    {
        while (await stream.ResponseStream.MoveNext(CancellationToken.None))
        {
            Console.WriteLine("Received : " + stream.ResponseStream.Current.Result);
        }
    });

    Greeting[] greetings =
    {
        new Greeting() { FirstName = "John", LastName = "Doe"},
        new Greeting() { FirstName = "John4", LastName = "Doe2"},
        new Greeting() { FirstName = "John3", LastName = "Doe3"},
        new Greeting() { FirstName = "John2", LastName = "Doe4"},
        new Greeting() { FirstName = "John1", LastName = "Doe5"},
    };

    foreach (var greeting in greetings)
    {
        Console.WriteLine("Sending : " + greeting.ToString());
        await stream.RequestStream.WriteAsync(new GreetingRequest() { Greeting = greeting });
    }

    await stream.RequestStream.CompleteAsync();
    await responseReaderTask;
}

static async Task ClientStreamGreet(Greeter.GreeterClient client, GreetingRequest request)
{
    var stream = client.LongGreet();

    foreach (int i in Enumerable.Range(1, 1000))
    {
        await stream.RequestStream.WriteAsync(request);
    }

    await stream.RequestStream.CompleteAsync();

    var response = await stream.ResponseAsync;

    Console.WriteLine(response.Result);
}

static async Task ServerStreamGreet(Greeter.GreeterClient client, GreetingRequest request)
{
    var response = client.GreetManyTimes(request);


    while (await response.ResponseStream.MoveNext(CancellationToken.None))
    {
        Console.WriteLine(response.ResponseStream.Current.Result);
    }
}

static async Task UrinaryGreet(Greeter.GreeterClient client)
{
    var reply = await client.SayHelloAsync(
        new HelloRequest { Name = "Jareb Smith" });

    Console.WriteLine(reply.Message);
}

