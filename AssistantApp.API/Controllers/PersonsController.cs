using AssistantApp.API.Services;
using AssistantApp.Shared.Models;
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
    public ActionResult<ApiResponse<List<Person>>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("search")]
    public ActionResult<ApiResponse<List<Person>>> Search(string term)
    {
        return Ok(_service.Search(term));
    }

    [HttpGet("{id}")]
    public ActionResult<ApiResponse<Person>> Get(int id)
    {
        var response = _service.GetById(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [HttpPost]
    public ActionResult<ApiResponse<Person>> Create(Person person)
    {
        var response = _service.Create(person);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public ActionResult<ApiResponse<Person>> Update(int id, Person person)
    {
        var response = _service.Update(id, person);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }
}