namespace AssistantApp.Shared.Models.Auth;

public class RolePermissionsBatchRequest
{
    public List<string> Add { get; set; } = new();
    public List<string> Remove { get; set; } = new();
}
