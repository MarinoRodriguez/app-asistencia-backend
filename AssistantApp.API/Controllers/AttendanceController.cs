using AssistantApp.API.Services;
using AssistantApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _service;

    public AttendanceController(AttendanceService service)
    {
        _service = service;
    }

    // GET: api/Attendance/event/5
    [HttpGet("event/{eventId}")]
    public ActionResult<ApiResponse<List<Assistance>>> GetByEvent(int eventId)
    {
        return Ok(_service.GetByEvent(eventId));
    }

    // GET: api/Attendance/event/5/roster
    [HttpGet("event/{eventId}/roster")]
    public ActionResult<ApiResponse<List<AttendanceRosterItem>>> GetRoster(int eventId)
    {
        var response = _service.GetRoster(eventId);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    // POST: api/Attendance/mark
    // Body: { eventId: 1, personId: 5, type: 1 }
    [HttpPost("mark")]
    public ActionResult<ApiResponse<Assistance>> Mark([FromBody] AttendanceRequest request)
    {
        var response = _service.MarkAttendance(request.EventId, request.PersonId, request.Type);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    // POST: api/Attendance/external/5
    // Body: Person object
    [HttpPost("external/{eventId}")]
    public ActionResult<ApiResponse<Assistance>> RegisterExternal(int eventId, [FromBody] Person person)
    {
        // Asumimos 'Present' por defecto para externos
        var response = _service.RegisterExternal(eventId, person, AssistanceType.Present);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    // DELETE: api/Attendance/event/5/person/10
    [HttpDelete("event/{eventId}/person/{personId}")]
    public ActionResult<ApiResponse<bool>> Remove(int eventId, int personId)
    {
        var response = _service.RemoveAttendance(eventId, personId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}

// DTO simple para la petición de marcar
public class AttendanceRequest
{
    public int EventId { get; set; }
    public int PersonId { get; set; }
    public AssistanceType Type { get; set; }
}
