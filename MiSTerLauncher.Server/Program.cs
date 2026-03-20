using MiSTerLauncher.Server.HostedService;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;


var builder = WebApplication.CreateBuilder(args);

// Clé JWT : chargée depuis data/jwt-key.json, générée automatiquement au premier lancement
const string jwtKeyFile = "data/jwt-key.json";
builder.Configuration.AddJsonFile(jwtKeyFile, optional: true, reloadOnChange: false);
if (string.IsNullOrEmpty(builder.Configuration["Jwt:Key"]))
{
    var newKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    Directory.CreateDirectory("data");
    File.WriteAllText(jwtKeyFile, System.Text.Json.JsonSerializer.Serialize(new { Jwt = new { Key = newKey } }));
    builder.Configuration["Jwt:Key"] = newKey;
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});

//builder.Services.AddHostedService<MisterBackgroundTask>();
builder.Services.AddSingleton<MisterBackgroundTask>();
builder.Services.AddHostedService<MisterBackgroundTask>(provider => provider.GetService<MisterBackgroundTask>());

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<MvcJsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

app.MapFallbackToFile("/index.html");

app.UseCors();
//app.UseWebSockets();

app.MapHub<MiSTerLauncher.Server.Hub.MisterHub>("/hub/misterhub", options =>
{
    options.Transports =
        HttpTransportType.WebSockets |
        HttpTransportType.LongPolling;
});



app.Run();
