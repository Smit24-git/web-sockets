using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// using web socket :)

var socketOptions = new WebSocketOptions{
    KeepAliveInterval = TimeSpan.FromSeconds(30) //default is 2 minutes
};

app.UseWebSockets(socketOptions);

app.Use(async(context, next) => {
    if(context.Request.Path == "/task-requester") {
        // Short-Circuit the Middleware Pipe. 
        if(context.WebSockets.IsWebSocketRequest) {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            Console.WriteLine("Socket Accepted Connection");
            var cancellationToken = context?.RequestAborted ?? CancellationToken.None;
            await HandleConnectionAsync(webSocket, cancellationToken);
        } else {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    } else { //not a web socket path, move to next middleware
        await next(context);
    }
});

async Task HandleConnectionAsync(WebSocket webSocket, CancellationToken cancellationToken) {
    var buffer = new byte[1024 * 8];

    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
    while(!receiveResult.CloseStatus.HasValue) {
        //send buffer message we received!

        //wait for new message
        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
    }
    
    if(receiveResult.CloseStatus.Value == WebSocketCloseStatus.EndpointUnavailable){
        // connection is aborted. no need to close the socket. socket should close itself automatically
    } else {
        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
    
    Console.WriteLine($@"Connection Closed with status: 
        {receiveResult.CloseStatus.Value}
        {receiveResult.CloseStatusDescription}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// boilder plate, remove later
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
