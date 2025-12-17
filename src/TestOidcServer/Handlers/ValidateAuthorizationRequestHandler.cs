using OpenIddict.Abstractions;
using OpenIddict.Server;
using TestOidcServer.Services;

namespace TestOidcServer.Handlers;

public class ValidateAuthorizationRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>
{
    private readonly FileClientStore _clientStore;

    public ValidateAuthorizationRequestHandler(FileClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    public ValueTask HandleAsync(OpenIddictServerEvents.ValidateAuthorizationRequestContext context)
    {
        var client = _clientStore.GetClientById(context.ClientId!);

        if (client is null)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidClient,
                description: "The specified client_id is not valid.");
            return ValueTask.CompletedTask;
        }

        // Validate redirect URI
        if (!string.IsNullOrEmpty(context.RedirectUri) &&
            !client.RedirectUris.Contains(context.RedirectUri, StringComparer.OrdinalIgnoreCase))
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidClient,
                description: "The specified redirect_uri is not valid for this client.");
            return ValueTask.CompletedTask;
        }

        return ValueTask.CompletedTask;
    }
}
