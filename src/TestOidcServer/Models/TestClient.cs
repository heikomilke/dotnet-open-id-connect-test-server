namespace TestOidcServer.Models;

public class TestClient
{
    public required string ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public required string DisplayName { get; set; }
    public List<string> RedirectUris { get; set; } = [];
    public List<string> PostLogoutRedirectUris { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}

public class ClientsConfiguration
{
    public List<TestClient> Clients { get; set; } = [];
}
