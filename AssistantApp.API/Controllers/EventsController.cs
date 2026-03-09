using AssistantApp.API.Services;
using AssistantApp.Shared;
using AssistantApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly EventService _service;

    public EventsController(EventService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.EventsView)]
    public ActionResult<ApiResponse<List<Event>>> GetAll([FromQuery] EventState? state)
    {
        return Ok(_service.GetAll(state));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.EventsView)]
    public ActionResult<ApiResponse<Event>> Get(int id)
    {
        var response = _service.GetById(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.EventsCreate)]
    public ActionResult<ApiResponse<Event>> Create(Event evt)
    {
        return Ok(_service.Create(evt));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.EventsEdit)]
    public ActionResult<ApiResponse<Event>> Update(int id, Event evt)
    {
        var response = _service.Update(id, evt);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost("{id}/start")]
    [Authorize(Policy = Permissions.EventsStart)]
    public ActionResult<ApiResponse<bool>> Start(int id)
    {
        var response = _service.StartEvent(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/finish")]
    [Authorize(Policy = Permissions.EventsClose)]
    public ActionResult<ApiResponse<bool>> Finish(int id)
    {
        var response = _service.FinishEvent(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/invite/person/{personId}")]
    [Authorize(Policy = Permissions.EventsEdit)]
    public ActionResult<ApiResponse<Invitation>> InvitePerson(int id, int personId)
    {
        var response = _service.InvitePerson(id, personId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/invite/group/{groupId}")]
    [Authorize(Policy = Permissions.EventsEdit)]
    public ActionResult<ApiResponse<int>> InviteGroup(int id, int groupId)
    {
        var response = _service.InviteGroup(id, groupId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}/invite/person/{personId}")]
    [Authorize(Policy = Permissions.EventsEdit)]
    public ActionResult<ApiResponse<bool>> RemoveInvitation(int id, int personId)
    {
        var response = _service.RemoveInvitation(id, personId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.EventsDelete)]
    public ActionResult<ApiResponse<bool>> Delete(int id)
    {
        var response = _service.Delete(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}
