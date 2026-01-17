using AssistantApp.API.Services;
using AssistantApp.Shared.Models;
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
    public ActionResult<ApiResponse<List<Group>>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<ApiResponse<Group>> Get(int id)
    {
        var response = _service.GetById(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    public ActionResult<ApiResponse<Group>> Create(Group group)
    {
        var response = _service.Create(group);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public ActionResult<ApiResponse<Group>> Update(int id, Group group)
    {
        var response = _service.Update(id, group);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public ActionResult<ApiResponse<bool>> Delete(int id)
    {
        var response = _service.Delete(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}