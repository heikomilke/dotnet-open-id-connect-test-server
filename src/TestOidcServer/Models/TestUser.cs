namespace TestOidcServer.Models;

public class TestUser
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

public class UsersConfiguration
{
    public List<TestUser> Users { get; set; } = [];
}
