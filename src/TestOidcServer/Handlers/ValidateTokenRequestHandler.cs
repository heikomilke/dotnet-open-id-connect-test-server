using OpenIddict.Abstractions;
using OpenIddict.Server;
using TestOidcServer.Services;

namespace TestOidcServer.Handlers;

public class ValidateTokenRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateTokenRequestContext>
{
    private readonly FileClientStore _clientStore;

    public ValidateTokenRequestHandler(FileClientStore clientStore)
    {
        _clientStore = clientStore;
    }

    public ValueTask HandleAsync(OpenIddictServerEvents.ValidateTokenRequestContext context)
    {
        var client = _clientStore.GetClientById(context.ClientId!);

        if (client is null)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidClient,
                description: "The specified client_id is not valid.");
            return ValueTask.CompletedTask;
        }

        // For confidential clients, validate the secret
        if (!string.IsNullOrEmpty(client.ClientSecret))
        {
            if (string.IsNullOrEmpty(context.ClientSecret) ||
                !string.Equals(context.ClientSecret, client.ClientSecret, StringComparison.Ordinal))
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidClient,
                    description: "The specified client credentials are invalid.");
                return ValueTask.CompletedTask;
            }
        }

        return ValueTask.CompletedTask;
    }
}
