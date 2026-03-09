namespace AssistantApp.Shared.Models.Auth;

public class LoginRequest
{
    public string EmailOrUserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
