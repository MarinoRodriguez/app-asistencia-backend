namespace AssistantApp.Shared.Models.Auth;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool LockedOut { get; set; }
    public List<string> Roles { get; set; } = new();
}
