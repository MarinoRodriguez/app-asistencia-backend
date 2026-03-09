using AssistantApp.API.Services;
using AssistantApp.Shared;
using AssistantApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GroupsController : ControllerBase
{
    private readonly GroupService _service;

    public GroupsController(GroupService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.GroupsView)]
    public ActionResult<ApiResponse<List<Group>>> GetAll([FromQuery] bool includeInactive = true)
    {
        return Ok(_service.GetAll(includeInactive));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.GroupsView)]
    public ActionResult<ApiResponse<Group>> Get(int id)
    {
        var response = _service.GetById(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.GroupsCreate)]
    public ActionResult<ApiResponse<Group>> Create(Group group)
    {
        var response = _service.Create(group);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.GroupsEdit)]
    public ActionResult<ApiResponse<Group>> Update(int id, Group group)
    {
        var response = _service.Update(id, group);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.GroupsDelete)]
    public ActionResult<ApiResponse<bool>> Delete(int id)
    {
        var response = _service.Delete(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}
