namespace AssistantApp.Shared.Models.Auth;

public class UpdateRolePermissionsRequest
{
    public List<string> Permissions { get; set; } = new();
}
