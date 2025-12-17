using System.Text.Json;
using TestOidcServer.Models;

namespace TestOidcServer.Services;

public class FileClientStore
{
    private readonly List<TestClient> _clients;

    public FileClientStore(IConfiguration configuration)
    {
        var configPath = configuration.GetValue<string>("ConfigPath") ?? "config";
        var clientsFile = Path.Combine(configPath, "clients.json");

        if (File.Exists(clientsFile))
        {
            var json = File.ReadAllText(clientsFile);
            var config = JsonSerializer.Deserialize<ClientsConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            _clients = config?.Clients ?? [];
        }
        else
        {
            _clients = [];
        }
    }

    public IReadOnlyList<TestClient> GetAllClients() => _clients;

    public TestClient? GetClientById(string clientId) =>
        _clients.FirstOrDefault(c => c.ClientId == clientId);
}
