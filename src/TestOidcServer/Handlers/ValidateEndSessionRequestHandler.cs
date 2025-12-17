using OpenIddict.Abstractions;
using OpenIddict.Server;
using TestOidcServer.Services;

namespace TestOidcServer.Handlers;

public class ValidateEndSessionRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateEndSessionRequestContext>
{
    private readonly FileClientStore _clientStore;

    public ValidateEndSessionRequestHandler(FileClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    public ValueTask HandleAsync(OpenIddictServerEvents.ValidateEndSessionRequestContext context)
    {
        // If a post_logout_redirect_uri is specified, validate it
        if (!string.IsNullOrEmpty(context.PostLogoutRedirectUri))
        {
            // Check if any client has this post logout redirect URI registered
            var clients = _clientStore.GetAllClients();
            var isValid = clients.Any(c =>
                c.PostLogoutRedirectUris.Contains(context.PostLogoutRedirectUri, StringComparer.OrdinalIgnoreCase));

            if (!isValid)
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidRequest,
                    description: "The specified post_logout_redirect_uri is not valid.");
                return ValueTask.CompletedTask;
            }
        }

        return ValueTask.CompletedTask;
    }
}
