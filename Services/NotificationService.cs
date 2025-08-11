using Microsoft.AspNetCore.SignalR;
using ToiletGhost.Hubs;
using ToiletGhost.Models;

namespace ToiletGhost.Services;

public interface INotificationService
{
    Task NotifyProjectCreated(Project project);
    Task NotifyProjectUpdated(Project project);
    Task NotifyProjectDeleted(string projectId);
    Task NotifyVersionAdded(ProjectVersion version);
    Task NotifyCommentAdded(Comment comment);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<ProjectHub> _hubContext;

    public NotificationService(IHubContext<ProjectHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyProjectCreated(Project project)
    {
        // Notify the project owner
        await _hubContext.Clients.Group($"user_{project.OwnerEmail}")
            .SendAsync("ProjectCreated", project);
        
        // Notify all users (for the main project list)
        await _hubContext.Clients.All
            .SendAsync("ProjectListUpdated");
    }

    public async Task NotifyProjectUpdated(Project project)
    {
        // Notify anyone viewing this project
        await _hubContext.Clients.Group($"project_{project.ProjectId}")
            .SendAsync("ProjectUpdated", project);
        
        // Notify the project owner
        await _hubContext.Clients.Group($"user_{project.OwnerEmail}")
            .SendAsync("ProjectUpdated", project);
        
        // Notify all users (for the main project list)
        await _hubContext.Clients.All
            .SendAsync("ProjectListUpdated");
    }

    public async Task NotifyProjectDeleted(string projectId)
    {
        // Notify anyone viewing this project
        await _hubContext.Clients.Group($"project_{projectId}")
            .SendAsync("ProjectDeleted", projectId);
        
        // Notify all users (for the main project list)
        await _hubContext.Clients.All
            .SendAsync("ProjectListUpdated");
    }

    public async Task NotifyVersionAdded(ProjectVersion version)
    {
        // Notify anyone viewing this project
        await _hubContext.Clients.Group($"project_{version.ProjectId}")
            .SendAsync("VersionAdded", version);
    }

    public async Task NotifyCommentAdded(Comment comment)
    {
        // Notify anyone viewing this project
        await _hubContext.Clients.Group($"project_{comment.ProjectId}")
            .SendAsync("CommentAdded", comment);
    }
}
