using Falcon.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder
    .WithOrigins("null")
    .AllowAnyHeader()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseRouting();

app.MapGet("/", () => "Hello World!");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chathub");
});

app.Run();