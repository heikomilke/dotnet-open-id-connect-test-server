using System.Text.Json;
using TestOidcServer.Models;

namespace TestOidcServer.Services;

public class FileUserStore
{
    private readonly List<TestUser> _users;

    public FileUserStore(IConfiguration configuration)
    {
        var configPath = configuration.GetValue<string>("ConfigPath") ?? "config";
        var usersFile = Path.Combine(configPath, "users.json");

        if (File.Exists(usersFile))
        {
            var json = File.ReadAllText(usersFile);
            var config = JsonSerializer.Deserialize<UsersConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            _users = config?.Users ?? [];
        }
        else
        {
            _users = [];
        }
    }

    public IReadOnlyList<TestUser> GetAllUsers() => _users;

    public TestUser? GetUserById(string id) => _users.FirstOrDefault(u => u.Id == id);

    public TestUser? GetUserByUsername(string username) =>
        _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
}
