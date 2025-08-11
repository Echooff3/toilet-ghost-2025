using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace ToiletGhost.Hubs;

[Authorize]
public class ProjectHub : Hub
{
    public async Task JoinProjectGroup(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }

    public async Task LeaveProjectGroup(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }

    public async Task JoinUserGroup(string userEmail)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userEmail}");
    }

    public async Task LeaveUserGroup(string userEmail)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userEmail}");
    }
}
