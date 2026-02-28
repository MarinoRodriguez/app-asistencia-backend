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

        // EF Core manejará la inserción de PersonGroups automáticamente si vienen en el objeto
        _context.People.Add(person);
        _context.SaveChanges();
        return ApiResponse<Person>.Ok(person, "Persona creada correctamente");
    }

    public ApiResponse<Person> Update(int id, Person person)
    {
        // Cargamos la entidad existente INCLUYENDO sus relaciones para poder modificarlas
        var existing = _context.People
            .Include(p => p.PersonGroups)
            .FirstOrDefault(p => p.Id == id);
            
        if (existing == null) return ApiResponse<Person>.Fail("Persona no encontrada");

        // 1. Actualizar campos básicos
        existing.Name = person.Name;
        existing.LastName = person.LastName;
        existing.Email = person.Email;
        existing.PhotoUrl = person.PhotoUrl;
        existing.IsCreatedAtRuntime = person.IsCreatedAtRuntime;

        // 2. Actualizar Relaciones (Grupos)
        // Eliminamos las relaciones actuales
        existing.PersonGroups.Clear();
        
        // Agregamos las nuevas (EF Core detectará que son nuevas relaciones)
        foreach (var pg in person.PersonGroups)
        {
            existing.PersonGroups.Add(new PersonGroup { PersonId = id, GroupId = pg.GroupId });
        }
        
        _context.SaveChanges();
        return ApiResponse<Person>.Ok(existing, "Persona actualizada");
    }

    public ApiResponse Delete(int id)
    {
        var existent = GetById(id);
        if (!existent.Success || existent.Data == null)
            return ApiResponse.Fail(existent.Message);

        _context.People.Remove(existent.Data);

        _context.SaveChanges();
        return ApiResponse.Ok();
    }
}