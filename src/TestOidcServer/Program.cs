using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using TestOidcServer.Handlers;
using TestOidcServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<FileUserStore>();
builder.Services.AddSingleton<FileClientStore>();

// Register custom event handlers
builder.Services.AddScoped<ValidateAuthorizationRequestHandler>();
builder.Services.AddScoped<ValidateTokenRequestHandler>();
builder.Services.AddScoped<ValidateEndSessionRequestHandler>();

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authorization/Login";
        options.LogoutPath = "/Authorization/Logout";
    });

// Configure OpenIddict in degraded mode (no database required)
builder.Services.AddOpenIddict()
    .AddServer(options =>
    {
        // Enable the required endpoints
        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token")
               .SetUserInfoEndpointUris("/connect/userinfo")
               .SetEndSessionEndpointUris("/connect/logout");

        // Enable the authorization code, refresh token, and client credentials flows
        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow()
               .AllowClientCredentialsFlow();

        // Register scopes
        options.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.Roles,
            "api"
        );

        // Use ephemeral signing/encryption keys (development only)
        options.AddEphemeralEncryptionKey()
               .AddEphemeralSigningKey();

        // Enable degraded mode to skip database requirement
        options.EnableDegradedMode();

        // Disable HTTPS requirement for development
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough()
               .DisableTransportSecurityRequirement();

        // Add custom event handlers for client validation
        options.AddEventHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>(builder =>
        {
            builder.UseScopedHandler<ValidateAuthorizationRequestHandler>();
            builder.SetOrder(100);
        });

        options.AddEventHandler<OpenIddictServerEvents.ValidateTokenRequestContext>(builder =>
        {
            builder.UseScopedHandler<ValidateTokenRequestHandler>();
            builder.SetOrder(100);
        });

        options.AddEventHandler<OpenIddictServerEvents.ValidateEndSessionRequestContext>(builder =>
        {
            builder.UseScopedHandler<ValidateEndSessionRequestHandler>();
            builder.SetOrder(100);
        });
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
