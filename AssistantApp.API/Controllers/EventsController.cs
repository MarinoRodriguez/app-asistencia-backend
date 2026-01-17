using AssistantApp.API.Services;
using AssistantApp.Shared.Models;
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
    public ActionResult<ApiResponse<List<Event>>> GetAll([FromQuery] EventState? state)
    {
        return Ok(_service.GetAll(state));
    }

    [HttpGet("{id}")]
    public ActionResult<ApiResponse<Event>> Get(int id)
    {
        var response = _service.GetById(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    public ActionResult<ApiResponse<Event>> Create(Event evt)
    {
        return Ok(_service.Create(evt));
    }

    [HttpPut("{id}")]
    public ActionResult<ApiResponse<Event>> Update(int id, Event evt)
    {
        var response = _service.Update(id, evt);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost("{id}/start")]
    public ActionResult<ApiResponse<bool>> Start(int id)
    {
        var response = _service.StartEvent(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/finish")]
    public ActionResult<ApiResponse<bool>> Finish(int id)
    {
        var response = _service.FinishEvent(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/invite/person/{personId}")]
    public ActionResult<ApiResponse<Invitation>> InvitePerson(int id, int personId)
    {
        var response = _service.InvitePerson(id, personId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("{id}/invite/group/{groupId}")]
    public ActionResult<ApiResponse<int>> InviteGroup(int id, int groupId)
    {
        var response = _service.InviteGroup(id, groupId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id}/invite/person/{personId}")]
    public ActionResult<ApiResponse<bool>> RemoveInvitation(int id, int personId)
    {
        var response = _service.RemoveInvitation(id, personId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}