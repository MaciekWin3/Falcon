using Falcon.Server;
using Falcon.Server.Features.Auth.Models;
using Falcon.Server.Features.Messages.Repositories;
using Falcon.Server.Features.Messages.Services;
using Falcon.Server.Hubs;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Security.Claims;
using System.Text;
using ILogger = Serilog.ILogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(
        CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();
//builder.WebHost
//    .UseKestrel()
//    //.UseUrls("http://192.168.240.1:5262", "https://localhost:7262", "http://localhost:5262")
//    .UseContentRoot(Directory.GetCurrentDirectory())
//    .ConfigureKestrel(options =>
//    {
//        options.Listen(IPAddress
//            .Parse(builder.Configuration["IpAddress:Address"]), int.Parse(builder.Configuration["IpAddress:Port"]));
//    })
//    .UseIISIntegration();

// Logger
builder.Logging.ClearProviders();
ILogger logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
    .CreateLogger();
builder.Logging.AddSerilog(logger);
builder.Services.AddSingleton(logger);

// Database
builder.Services.AddEntityFrameworkNpgsql()
    .AddDbContext<FalconDbContext>(options => options
    .UseLazyLoadingProxies()
    .UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));

// Auth - checkParameters
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<FalconDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chathub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Repositories
builder.Services.AddTransient<IMessageRepository, MessageRepository>();

// Services
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddSingleton<IDictionary<string, UserConnection>>(x => new Dictionary<string, UserConnection>());
builder.Services.AddSingleton(x => new HashSet<string>() { "All", "Create new room" });

// Swagger
//builder.Services.AddSwaggerGen(options =>
//{
//    var apiInfo = new OpenApiInfo { Title = "TestWebApi", Version = "v1" };
//    options.SwaggerDoc("controllers", apiInfo);
//    options.SwaggerDoc("hubs", apiInfo);
//    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Falcon", Version = "v1" });

//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = @"JWT",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Scheme = "Bearer",
//        Type = SecuritySchemeType.Http,
//    });

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                },
//                Name = "Bearer",
//                In = ParameterLocation.Header
//            },
//            new List<string>()
//        }
//    });

//    options.AddSignalRSwaggerGen();
//});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/swagger/controllers/swagger.json", "REST API");
    //    options.SwaggerEndpoint("/swagger/hubs/swagger.json", "SignalR");
    //});
}
else
{
    //app.UseHttpsRedirection();
}

app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = "/index",
    EnableDefaultFiles = true
});

app.UseCors(builder => builder
    .WithOrigins("null")
    .AllowAnyHeader()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/index.html"));

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub");
});

app.Run();