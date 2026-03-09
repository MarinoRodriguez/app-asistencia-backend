using AssistantApp.API.Identity;
using AssistantApp.Shared;
using AssistantApp.Shared.Models;
using AssistantApp.Shared.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssistantApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.UsersView)]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll([FromQuery] string? search = null)
    {
        var query = userManager.Users;
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                (u.UserName ?? "").Contains(search) ||
                (u.Email ?? "").Contains(search));
        }

        var users = query.ToList();
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                LockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                Roles = roles.ToList()
            });
        }

        return Ok(ApiResponse<List<UserDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.UsersCreate)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Email y contraseña son obligatorios"));
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
            return BadRequest(ApiResponse<UserDto>.Fail(msg));
        }

        if (request.Roles.Any())
        {
            var validRoles = new List<string>();
            foreach (var role in request.Roles.Distinct())
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    validRoles.Add(role);
                }
            }

            if (validRoles.Any())
            {
                await userManager.AddToRolesAsync(user, validRoles);
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var dto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            LockedOut = false,
            Roles = roles.ToList()
        };

        return Ok(ApiResponse<UserDto>.Ok(dto));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("Usuario no encontrado"));
        }

        user.UserName = string.IsNullOrWhiteSpace(request.UserName) ? user.UserName : request.UserName;
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? user.Email : request.Email;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse<UserDto>.Fail(msg));
        }

        var roles = await userManager.GetRolesAsync(user);
        var dto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            LockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
            Roles = roles.ToList()
        };

        return Ok(ApiResponse<UserDto>.Ok(dto));
    }

    [HttpPut("{id}/lock")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<ActionResult<ApiResponse>> SetLock(string id, [FromBody] UpdateUserLockRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("Usuario no encontrado"));
        }

        if (request.Locked)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        else
        {
            await userManager.SetLockoutEndDateAsync(user, null);
        }

        return Ok(ApiResponse.Ok("Estado de acceso actualizado"));
    }

    [HttpPut("{id}/roles")]
    [Authorize(Policy = Permissions.UsersRolesManage)]
    public async Task<ActionResult<ApiResponse>> UpdateRoles(string id, [FromBody] UpdateUserRolesRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("Usuario no encontrado"));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var desiredRoles = request.Roles.Distinct().ToList();

        var rolesToRemove = currentRoles.Except(desiredRoles);
        var rolesToAdd = desiredRoles.Except(currentRoles).ToList();

        if (rolesToRemove.Any())
        {
            await userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        if (rolesToAdd.Any())
        {
            var validRoles = new List<string>();
            foreach (var role in rolesToAdd)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    validRoles.Add(role);
                }
            }

            if (validRoles.Any())
            {
                await userManager.AddToRolesAsync(user, validRoles);
            }
        }

        return Ok(ApiResponse.Ok("Roles actualizados"));
    }

    [HttpPost("{id}/reset-password")]
    [Authorize(Policy = Permissions.UsersResetPassword)]
    public async Task<ActionResult<ApiResponse>> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(ApiResponse.Fail("Nueva contraseña requerida"));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("Usuario no encontrado"));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse.Fail(msg));
        }

        return Ok(ApiResponse.Ok("Contraseña restablecida"));
    }

    [HttpPost("me/change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangeMyPassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse.Fail("No autenticado"));
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized(ApiResponse.Fail("No autenticado"));
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse.Fail(msg));
        }

        return Ok(ApiResponse.Ok("Contraseña cambiada"));
    }
}
