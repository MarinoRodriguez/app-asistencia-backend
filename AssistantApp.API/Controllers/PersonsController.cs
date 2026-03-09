using AssistantApp.API.Services;
using AssistantApp.Shared;
using AssistantApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssistantApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonsController : ControllerBase
{
    private readonly PersonService _service;

    public PersonsController(PersonService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.PersonsView)]
    public ActionResult<ApiResponse<List<Person>>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("search")]
    [Authorize(Policy = Permissions.PersonsView)]
    public ActionResult<ApiResponse<List<Person>>> Search(string term)
    {
        return Ok(_service.Search(term));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.PersonsView)]
    public ActionResult<ApiResponse<Person>> Get(int id)
    {
        var response = _service.GetById(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PersonsCreateAdmin)]
    public ActionResult<ApiResponse<Person>> Create(Person person)
    {
        var response = _service.Create(person);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.PersonsEdit)]
    public ActionResult<ApiResponse<Person>> Update(int id, Person person)
    {
        var response = _service.Update(id, person);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.PersonsDelete)]
    public ActionResult<ApiResponse<Person>> Delete(int id)
    {
        var response = _service.Delete(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}
