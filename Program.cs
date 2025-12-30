using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using bitsbybeier.Api.Configuration;
using bitsbybeier.Api.Services;
using bitsbybeier.Api.Mcp;
using bitsbybeier.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BitsbyBeier API",
        Version = "v1",
        Description = "API for content management and MCP server integration"
    });

    // Configure OAuth2 (Google) authentication for Swagger - Simpler option
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "User Profile" },
                    { "email", "User Email" }
                }
            }
        },
        Description = "Google OAuth2 authentication - click Authorize to login with Google"
    });

    // Configure JWT Bearer authentication for Swagger - Manual token option
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token in the text input below. Alternative to OAuth2 login."
    });

    // Add security requirement - endpoints can use either OAuth2 or Bearer
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "openid", "profile", "email" }
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Options pattern
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<GoogleAuthOptions>(
    builder.Configuration.GetSection(GoogleAuthOptions.SectionName));

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Database Initializer
builder.Services.AddScoped<DatabaseInitializer>();

// Add Application Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContentService, ContentService>();

// Add MCP Server
builder.Services.AddMcpServer()
    .WithTools<ContentMcpTools>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:4200" };
        
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configure Authentication
var jwtSecret = builder.Configuration["Authentication:Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret must be configured. Set the Authentication:Jwt:Secret configuration value or JWT_SECRET environment variable.");
var jwtIssuer = builder.Configuration["Authentication:Jwt:Issuer"] ?? "bitsbybeier";
var jwtAudience = builder.Configuration["Authentication:Jwt:Audience"] ?? "bitsbybeier-app";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
});

// Add Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Initialize database with migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BitsbyBeier API V1");
        
        // Configure OAuth2 for Google authentication
        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        if (!string.IsNullOrEmpty(googleClientId))
        {
            options.OAuthClientId(googleClientId);
            options.OAuthAppName("BitsbyBeier Swagger UI");
            options.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        }
    });
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add Cross-Origin-Opener-Policy header for Google Sign-In compatibility
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
