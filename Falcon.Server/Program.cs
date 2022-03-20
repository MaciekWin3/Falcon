using Falcon.Server;
using Falcon.Server.Features.Auth.Models;
using Falcon.Server.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDbContext<FalconDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FalconDb"));
});

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
            ClockSkew = TimeSpan.Zero
        };
    });

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