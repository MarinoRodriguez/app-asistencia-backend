using AssistantApp.API.Data;
using AssistantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AssistantApp.API.Services;

public class PersonService
{
    private readonly AppDbContext _context;

    public PersonService(AppDbContext context)
    {
        _context = context;
    }

    public ApiResponse<List<Person>> GetAll()
    {
        try
        {
            var list = _context.People
                .Include(p => p.PersonGroups)
                .ThenInclude(pg => pg.Group)
                .ToList();
            return ApiResponse<List<Person>>.Ok(list);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<Person>>.Fail(ex.Message);
        }
    }

    public ApiResponse<List<Person>> Search(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return ApiResponse<List<Person>>.Fail("Término de búsqueda vacío");

        var normalized = term.ToLower();
        var list = _context.People
            .Where(p => p.Name.ToLower().Contains(normalized) || 
                        p.LastName.ToLower().Contains(normalized) ||
                        (p.Email != null && p.Email.ToLower().Contains(normalized)))
            .Include(p => p.PersonGroups)
            .ThenInclude(pg => pg.Group)
            .ToList();

        return ApiResponse<List<Person>>.Ok(list);
    }

    public ApiResponse<Person> GetById(int id)
    {
        var person = _context.People
            .Include(p => p.PersonGroups)
            .ThenInclude(pg => pg.Group)
            .FirstOrDefault(p => p.Id == id);

        if (person == null) return ApiResponse<Person>.Fail("Persona no encontrada");
        return ApiResponse<Person>.Ok(person);
    }

    public ApiResponse<Person> Create(Person person)
    {
        if (!string.IsNullOrEmpty(person.Email) && _context.People.Any(p => p.Email == person.Email))
            return ApiResponse<Person>.Fail("El email ya existe");

        _context.People.Add(person);
        _context.SaveChanges();
        return ApiResponse<Person>.Ok(person, "Persona creada correctamente");
    }

    public ApiResponse<Person> Update(int id, Person person)
    {
        var existing = _context.People.Find(id);
        if (existing == null) return ApiResponse<Person>.Fail("Persona no encontrada");

        // Actualizamos campos básicos
        existing.Name = person.Name;
        existing.LastName = person.LastName;
        existing.Email = person.Email;
        existing.PhotoUrl = person.PhotoUrl;
        existing.IsCreatedAtRuntime = person.IsCreatedAtRuntime; // CORREGIDO
        
        _context.SaveChanges();
        return ApiResponse<Person>.Ok(existing, "Persona actualizada");
    }
}