using AssistantApp.API.Identity;
using AssistantApp.Shared;
using AssistantApp.Shared.Models;
using AssistantApp.Shared.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AssistantApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<LoginResponse>.Fail("Email y contraseña son obligatorios"));
        }

        var hasAnyUser = await userManager.Users.AnyAsync();
        if (hasAnyUser)
        {
            return Forbid();
        }

        var user = new ApplicationUser
        {
            UserName = string.IsNullOrWhiteSpace(request.UserName) ? request.Email : request.UserName,
            Email = request.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse<LoginResponse>.Fail(msg));
        }

        await EnsureAdminRoleWithPermissions();
        await userManager.AddToRoleAsync(user, "Admin");

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsFromRoles(roles);
        var tokenResult = CreateJwtToken(user, roles, permissions);

        var response = new LoginResponse
        {
            Token = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            Permissions = permissions.ToList()
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Usuario administrador creado"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<LoginResponse>.Fail("Credenciales inválidas"));
        }

        var user = await userManager.FindByNameAsync(request.EmailOrUserName)
                   ?? await userManager.FindByEmailAsync(request.EmailOrUserName);

        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Credenciales inválidas"));
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsFromRoles(roles);

        var tokenResult = CreateJwtToken(user, roles, permissions);

        var response = new LoginResponse
        {
            Token = tokenResult.Token,
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            Permissions = permissions.ToList()
        };

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

    private (string Token, DateTime ExpiresAtUtc) CreateJwtToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var key = config["Jwt:Key"] ?? "dev-change-this-key";
        var issuer = config["Jwt:Issuer"] ?? "AssistantApp";
        var audience = config["Jwt:Audience"] ?? "AssistantApp";
        var expiresMinutes = int.TryParse(config["Jwt:ExpiresMinutes"], out var minutes) ? minutes : 480;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private async Task<HashSet<string>> GetPermissionsFromRoles(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>();
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var claims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == "permission"))
            {
                permissions.Add(claim.Value);
            }
        }

        return permissions;
    }

    private async Task EnsureAdminRoleWithPermissions()
    {
        var adminRoleName = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        var adminRole = await roleManager.FindByNameAsync(adminRoleName);
        if (adminRole == null)
        {
            return;
        }

        var existingClaims = await roleManager.GetClaimsAsync(adminRole);
        foreach (var permission in Permissions.All)
        {
            if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
            {
                await roleManager.AddClaimAsync(adminRole, new Claim("permission", permission));
            }
        }
    }
}
