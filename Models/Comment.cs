using Azure;
using Azure.Data.Tables;

namespace ToiletGhost.Models;

public enum CommentType
{
    Text,
    Image,
    Link
}

public class Comment : ITableEntity
{
    public string PartitionKey { get; set; } = "comments";
    public string RowKey { get; set; } = string.Empty; // ProjectId-Timestamp
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ProjectId { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public CommentType Type { get; set; }
    public string CommentData { get; set; } = string.Empty; // For images, this stores the original image blob name
    public string? ThumbnailBlobName { get; set; } // For images, this stores the thumbnail blob name
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Comment() { }

    public Comment(string projectId, string nickname, CommentType type, string commentData)
    {
        ProjectId = projectId;
        Nickname = nickname;
        Type = type;
        CommentData = commentData;
        RowKey = $"{projectId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }
}
