using System.Security.Claims;
using System.Text;
using JwtAspNetBlazor;
using JwtAspNetBlazor.Models;
using JwtAspNetBlazor.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.PrivateKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("manager"));
    options.AddPolicy("Employee", policy => policy.RequireRole("employee"));
});

builder.Services.AddTransient<TokenService>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/hello", (HttpContext context)
        => Results.Ok(context.User.Identity?.Name ?? string.Empty))
    .RequireAuthorization();

app.MapPost("/login", (User user, TokenService tokenService)
    => tokenService.Generate(user));

app.MapGet("/employee",
        (ClaimsPrincipal user) =>
            Results.Ok(new { message = $"Authenticated as {user.Identity?.Name}" }))
    .RequireAuthorization("Employee");

app.MapGet("/manager",
        (ClaimsPrincipal user) =>
            Results.Ok(new { message = $"Authenticated as {user.Identity?.Name}" }))
    .RequireAuthorization("Admin");

app.Run();