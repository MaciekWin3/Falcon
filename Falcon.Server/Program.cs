using Falcon.Server;
using Falcon.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    var apiInfo = new OpenApiInfo { Title = "TestWebApi", Version = "v1" };
    options.SwaggerDoc("controllers", apiInfo);
    options.SwaggerDoc("hubs", apiInfo);
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Falcon", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    options.AddSignalRSwaggerGen();
});

builder.Services.AddDbContext<FalconDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FalconDb"));
});

// Auth
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/controllers/swagger.json", "REST API");
        options.SwaggerEndpoint("/swagger/hubs/swagger.json", "SignalR");
    });
}

app.UseCors(builder => builder
    .WithOrigins("null")
    .AllowAnyHeader()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello Falcon!");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub");
});

app.Run();