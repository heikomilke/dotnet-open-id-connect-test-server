using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using TestOidcServer.Models;
using TestOidcServer.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TestOidcServer.Controllers;

public class AuthorizationController : Controller
{
    private readonly FileUserStore _userStore;
    private readonly FileClientStore _clientStore;

    public AuthorizationController(FileUserStore userStore, FileClientStore clientStore)
    {
        _userStore = userStore;
        _clientStore = clientStore;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // If the user is not authenticated, show the login page
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            return View("Login", new LoginViewModel
            {
                Users = _userStore.GetAllUsers().ToList(),
                ReturnUrl = Request.PathBase + Request.Path + Request.QueryString
            });
        }

        // Create the claims principal with the user's claims
        var userId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = _userStore.GetUserById(userId!);
        if (user is null)
        {
            return View("Login", new LoginViewModel
            {
                Users = _userStore.GetAllUsers().ToList(),
                ReturnUrl = Request.PathBase + Request.Path + Request.QueryString
            });
        }

        var claims = new List<Claim>
        {
            new(Claims.Subject, user.Id),
            new(Claims.Name, user.Username),
            new(Claims.Email, user.Email)
        };

        // Add custom claims from the user configuration
        foreach (var claim in user.Claims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Set the scopes
        identity.SetScopes(request.GetScopes());
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/Authorization/Login")]
    public async Task<IActionResult> Login(string userId, string returnUrl)
    {
        var user = _userStore.GetUserById(userId);
        if (user is null)
        {
            return BadRequest("User not found");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect("/");
    }

    [HttpGet("~/Authorization/Login")]
    public IActionResult LoginPage(string? returnUrl = null)
    {
        return View("Login", new LoginViewModel
        {
            Users = _userStore.GetAllUsers().ToList(),
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            var client = _clientStore.GetClientById(request.ClientId!);
            if (client is null)
            {
                throw new InvalidOperationException("The application cannot be found.");
            }

            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name,
                Claims.Role);

            identity.SetClaim(Claims.Subject, client.ClientId);
            identity.SetClaim(Claims.Name, client.DisplayName);

            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var userId = result.Principal?.GetClaim(Claims.Subject);

            if (string.IsNullOrEmpty(userId))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                    }));
            }

            var user = _userStore.GetUserById(userId);
            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user no longer exists."
                    }));
            }

            var identity = new ClaimsIdentity(result.Principal!.Claims,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name,
                Claims.Role);

            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    public IActionResult Userinfo()
    {
        var userId = User.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is invalid."
                }));
        }

        var user = _userStore.GetUserById(userId);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user no longer exists."
                }));
        }

        var claims = new Dictionary<string, object>
        {
            [Claims.Subject] = user.Id,
            [Claims.Name] = user.Username,
            [Claims.Email] = user.Email
        };

        // Add custom claims from user configuration
        foreach (var claim in user.Claims)
        {
            claims[claim.Key] = claim.Value;
        }

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.Email:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Profile) == true ||
                    claim.Subject?.HasScope(Scopes.Email) == true)
                {
                    yield return Destinations.IdentityToken;
                }
                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Roles) == true)
                {
                    yield return Destinations.IdentityToken;
                }
                yield break;

            // Custom claims go to both tokens
            default:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;
        }
    }
}

public class LoginViewModel
{
    public List<TestUser> Users { get; set; } = [];
    public string? ReturnUrl { get; set; }
}
