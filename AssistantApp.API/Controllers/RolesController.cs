using AssistantApp.Shared;
using AssistantApp.Shared.Models;
using AssistantApp.Shared.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(RoleManager<IdentityRole> roleManager) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.RolesView)]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll()
    {
        var roles = roleManager.Roles.ToList();
        var result = new List<RoleDto>();
        foreach (var role in roles)
        {
            var claims = await roleManager.GetClaimsAsync(role);
            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList()
            });
        }

        return Ok(ApiResponse<List<RoleDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse<RoleDto>.Fail("Nombre de rol requerido"));
        }

        if (await roleManager.RoleExistsAsync(request.Name))
        {
            return BadRequest(ApiResponse<RoleDto>.Fail("El rol ya existe"));
        }

        var create = await roleManager.CreateAsync(new IdentityRole(request.Name));
        if (!create.Succeeded)
        {
            var msg = string.Join("; ", create.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse<RoleDto>.Fail(msg));
        }

        var role = await roleManager.FindByNameAsync(request.Name);
        var dto = new RoleDto
        {
            Id = role?.Id ?? string.Empty,
            Name = role?.Name ?? request.Name,
            Permissions = new List<string>()
        };

        return Ok(ApiResponse<RoleDto>.Ok(dto));
    }

    [HttpPut("{id}/permissions")]
    [Authorize(Policy = Permissions.RolePermissionsManage)]
    public async Task<ActionResult<ApiResponse>> UpdatePermissions(string id, [FromBody] UpdateRolePermissionsRequest request)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound(ApiResponse.Fail("Rol no encontrado"));
        }

        var currentClaims = await roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims.Where(c => c.Type == "permission").Select(c => c.Value).ToHashSet();
        var desiredPermissions = request.Permissions.Distinct().ToHashSet();

        var toRemove = currentPermissions.Except(desiredPermissions);
        var toAdd = desiredPermissions.Except(currentPermissions);

        foreach (var permission in toRemove)
        {
            await roleManager.RemoveClaimAsync(role, currentClaims.First(c => c.Type == "permission" && c.Value == permission));
        }

        foreach (var permission in toAdd)
        {
            await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
        }

        return Ok(ApiResponse.Ok("Permisos actualizados"));
    }

    [HttpPost("{id}/permissions/batch")]
    [Authorize(Policy = Permissions.RolePermissionsManage)]
    public async Task<ActionResult<ApiResponse>> UpdatePermissionsBatch(string id, [FromBody] RolePermissionsBatchRequest request)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound(ApiResponse.Fail("Rol no encontrado"));
        }

        var currentClaims = await roleManager.GetClaimsAsync(role);
        var currentPermissions = currentClaims.Where(c => c.Type == "permission").Select(c => c.Value).ToHashSet();

        var toAdd = request.Add.Distinct().Where(p => !currentPermissions.Contains(p));
        var toRemove = request.Remove.Distinct().Where(p => currentPermissions.Contains(p));

        foreach (var permission in toAdd)
        {
            await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
        }

        foreach (var permission in toRemove)
        {
            var claim = currentClaims.First(c => c.Type == "permission" && c.Value == permission);
            await roleManager.RemoveClaimAsync(role, claim);
        }

        return Ok(ApiResponse.Ok("Permisos actualizados"));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.RolesDelete)]
    public async Task<ActionResult<ApiResponse>> Delete(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound(ApiResponse.Fail("Rol no encontrado"));
        }

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse.Fail(msg));
        }

        return Ok(ApiResponse.Ok("Rol eliminado"));
    }

    [HttpGet("permissions")]
    [Authorize(Policy = Permissions.RolesView)]
    public ActionResult<ApiResponse<List<string>>> GetPermissions()
    {
        return Ok(ApiResponse<List<string>>.Ok(Permissions.All.ToList()));
    }
}
