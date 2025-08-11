using Azure;
using Azure.Data.Tables;

namespace ToiletGhost.Models;

public class User : ITableEntity
{
    public string PartitionKey { get; set; } = "users";
    public string RowKey { get; set; } = string.Empty; // Email
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User() { }

    public User(string email)
    {
        Email = email;
        RowKey = email;
        Nickname = $"Ghost#{Random.Shared.Next(1000, 9999)}";
    }
}
