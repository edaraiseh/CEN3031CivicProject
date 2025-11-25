using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Services;
using Api.Controllers;
using Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var frontendOrigin = builder.Configuration
    .GetSection("FrontendOrigin")
    .Get<string>();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(frontendOrigin!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Configuring JWT
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings") // reads from appsettings.json "JwtSettings" section
);
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

var key = Encoding.ASCII.GetBytes(jwtSettings!.Key);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // set to true if you have an issuer
        ValidIssuer = "TownVoice",
        ValidateAudience = true, // set to true if you have an audience
        ValidAudience = "TownVoice",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostsService>();
builder.Services.AddScoped<PetitionsService>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IOptions<JwtSettings>>().Value);
builder.Services.AddScoped<JwtService>();
builder.Services.AddControllers();
builder.Services.AddDbContext<TVDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }  // Allows for E2E tests using the actual server