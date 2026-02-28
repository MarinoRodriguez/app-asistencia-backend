using AssistantApp.API.Data;
using AssistantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AssistantApp.API.Services;

public class GroupService
{
    private readonly AppDbContext _context;

    public GroupService(AppDbContext context)
    {
        _context = context;
    }

    public ApiResponse<List<Group>> GetAll(bool includeInactive = true)
    {
        var query = _context.Groups
            .Include(g => g.PersonGroups)
            .ThenInclude(pg => pg.Person)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(g => g.Active);
        }

        var list = query.ToList();
        return ApiResponse<List<Group>>.Ok(list);
    }

    public ApiResponse<Group> GetById(int id)
    {
        var group = _context.Groups
            .Include(g => g.PersonGroups)
            .ThenInclude(pg => pg.Person)
            .FirstOrDefault(g => g.Id == id);
            
        if (group == null) return ApiResponse<Group>.Fail("Grupo no encontrado");
        return ApiResponse<Group>.Ok(group);
    }

    public ApiResponse<Group> Create(Group group)
    {
        if (_context.Groups.Any(g => g.Name == group.Name))
            return ApiResponse<Group>.Fail("Ya existe un grupo con ese nombre");

        group.Active = true; // Por defecto activo
        _context.Groups.Add(group);
        _context.SaveChanges();
        return ApiResponse<Group>.Ok(group, "Grupo creado");
    }

    public ApiResponse<Group> Update(int id, Group group)
    {
        var existing = _context.Groups.Find(id);
        if (existing == null) return ApiResponse<Group>.Fail("Grupo no encontrado");

        existing.Name = group.Name;
        existing.Description = group.Description;
        existing.Active = group.Active; // Permitir activar/desactivar

        _context.SaveChanges();
        return ApiResponse<Group>.Ok(existing, "Grupo actualizado");
    }

    public ApiResponse<bool> Delete(int id)
    {
        // Soft Delete
        var group = _context.Groups.Find(id);
        if (group == null) return ApiResponse<bool>.Fail("Grupo no encontrado");
        _context.Groups.Remove(group);
        _context.SaveChanges();
        return ApiResponse<bool>.Ok(true, "Grupo desactivado");
    }
}