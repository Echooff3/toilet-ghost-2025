using Azure.Data.Tables;
using ToiletGhost.Models;

namespace ToiletGhost.Services;

public interface ITableStorageService
{
    Task<User?> GetUserAsync(string email);
    Task<User> CreateOrUpdateUserAsync(User user);
    Task<IEnumerable<User>> GetAllUsersAsync();
    
    Task<Project?> GetProjectAsync(string projectId);
    Task<Project> CreateProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task<IEnumerable<Project>> GetUserProjectsAsync(string ownerEmail);
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<bool> DeleteProjectAsync(string projectId);
    
    Task<ProjectVersion> CreateVersionAsync(ProjectVersion version);
    Task<IEnumerable<ProjectVersion>> GetProjectVersionsAsync(string projectId);
    Task<ProjectVersion?> GetLatestVersionAsync(string projectId);
    Task<bool> DeleteVersionAsync(string projectId, long versionNumber);
    
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<IEnumerable<Comment>> GetProjectCommentsAsync(string projectId);
    Task<bool> DeleteCommentAsync(string projectId, long timestamp);
}

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly TableClient _usersTable;
    private readonly TableClient _projectsTable;
    private readonly TableClient _versionsTable;
    private readonly TableClient _commentsTable;

    public TableStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureTables") ?? 
                              configuration["AzureTables:ConnectionString"];
        
        _tableServiceClient = new TableServiceClient(connectionString);
        _usersTable = _tableServiceClient.GetTableClient("Users");
        _projectsTable = _tableServiceClient.GetTableClient("Projects");
        _versionsTable = _tableServiceClient.GetTableClient("ProjectVersions");
        _commentsTable = _tableServiceClient.GetTableClient("Comments");

        // Create tables if they don't exist
        _ = Task.Run(async () =>
        {
            await _usersTable.CreateIfNotExistsAsync();
            await _projectsTable.CreateIfNotExistsAsync();
            await _versionsTable.CreateIfNotExistsAsync();
            await _commentsTable.CreateIfNotExistsAsync();
        });
    }

    // User methods
    public async Task<User?> GetUserAsync(string email)
    {
        try
        {
            var response = await _usersTable.GetEntityAsync<User>("users", email);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<User> CreateOrUpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _usersTable.UpsertEntityAsync(user);
        return user;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = new List<User>();
        await foreach (var user in _usersTable.QueryAsync<User>(filter: "PartitionKey eq 'users'"))
        {
            users.Add(user);
        }
        return users.OrderBy(u => u.Nickname);
    }

    // Project methods
    public async Task<Project?> GetProjectAsync(string projectId)
    {
        try
        {
            var response = await _projectsTable.GetEntityAsync<Project>("projects", projectId);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        await _projectsTable.AddEntityAsync(project);
        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        await _projectsTable.UpdateEntityAsync(project, project.ETag);
        return project;
    }

    public async Task<IEnumerable<Project>> GetUserProjectsAsync(string ownerEmail)
    {
        var projects = new List<Project>();
        await foreach (var project in _projectsTable.QueryAsync<Project>(filter: $"PartitionKey eq 'projects' and OwnerEmail eq '{ownerEmail}'"))
        {
            projects.Add(project);
        }
        return projects.OrderByDescending(p => p.UpdatedAt);
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        var projects = new List<Project>();
        await foreach (var project in _projectsTable.QueryAsync<Project>(filter: "PartitionKey eq 'projects'"))
        {
            projects.Add(project);
        }
        return projects.OrderByDescending(p => p.UpdatedAt);
    }

    public async Task<bool> DeleteProjectAsync(string projectId)
    {
        try
        {
            await _projectsTable.DeleteEntityAsync("projects", projectId);
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    // Version methods
    public async Task<ProjectVersion> CreateVersionAsync(ProjectVersion version)
    {
        await _versionsTable.AddEntityAsync(version);
        return version;
    }

    public async Task<IEnumerable<ProjectVersion>> GetProjectVersionsAsync(string projectId)
    {
        var versions = new List<ProjectVersion>();
        await foreach (var version in _versionsTable.QueryAsync<ProjectVersion>(filter: $"PartitionKey eq 'versions' and ProjectId eq '{projectId}'"))
        {
            versions.Add(version);
        }
        return versions.OrderByDescending(v => v.VersionNumber);
    }

    public async Task<ProjectVersion?> GetLatestVersionAsync(string projectId)
    {
        var versions = await GetProjectVersionsAsync(projectId);
        return versions.FirstOrDefault();
    }

    public async Task<bool> DeleteVersionAsync(string projectId, long versionNumber)
    {
        try
        {
            await _versionsTable.DeleteEntityAsync("versions", $"{projectId}-{versionNumber}");
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    // Comment methods
    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        await _commentsTable.AddEntityAsync(comment);
        return comment;
    }

    public async Task<IEnumerable<Comment>> GetProjectCommentsAsync(string projectId)
    {
        var comments = new List<Comment>();
        await foreach (var comment in _commentsTable.QueryAsync<Comment>(filter: $"PartitionKey eq 'comments' and ProjectId eq '{projectId}'"))
        {
            comments.Add(comment);
        }
        return comments.OrderBy(c => c.CreatedAt);
    }

    public async Task<bool> DeleteCommentAsync(string projectId, long timestamp)
    {
        try
        {
            await _commentsTable.DeleteEntityAsync("comments", $"{projectId}-{timestamp}");
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
