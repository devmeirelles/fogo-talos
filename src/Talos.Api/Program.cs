var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "F.O.G.O. Talos API");

app.Run();
