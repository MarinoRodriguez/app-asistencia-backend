namespace AssistantApp.Shared.Models.Auth;

public class UpdateUserRolesRequest
{
    public List<string> Roles { get; set; } = new();
}
