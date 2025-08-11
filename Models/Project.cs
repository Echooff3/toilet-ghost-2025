using Azure;
using Azure.Data.Tables;

namespace ToiletGhost.Models;

public class Project : ITableEntity
{
    public string PartitionKey { get; set; } = "projects";
    public string RowKey { get; set; } = string.Empty; // ProjectId (GUID)
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ProjectId { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArtworkBlobName { get; set; } // Original artwork blob name
    public string? ArtworkThumbnailBlobName { get; set; } // Thumbnail artwork blob name
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Project() { }

    public Project(string ownerEmail, string name)
    {
        ProjectId = Guid.NewGuid().ToString();
        RowKey = ProjectId;
        OwnerEmail = ownerEmail;
        Name = name;
    }
}
