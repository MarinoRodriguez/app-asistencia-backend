using AssistantApp.API.Data;
using AssistantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AssistantApp.API.Services;

public class EventService
{
    private readonly AppDbContext _context;

    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public ApiResponse<List<Event>> GetAll(EventState? state)
    {
        var query = _context.Events.AsQueryable();
        if (state.HasValue)
        {
            query = query.Where(e => e.State == state.Value);
        }

        var list = query.OrderByDescending(e => e.ScheduledStartDate).ToList();
        return ApiResponse<List<Event>>.Ok(list);
    }

    public ApiResponse<Event> GetById(int id)
    {
        var evt = _context.Events
            .Include(e => e.Invitations)
            .ThenInclude(i => i.Person)
            .FirstOrDefault(e => e.Id == id);

        if (evt == null) return ApiResponse<Event>.Fail("Evento no encontrado");
        return ApiResponse<Event>.Ok(evt);
    }

    public ApiResponse<Event> Create(Event evt)
    {
        if (evt.State == 0) evt.State = EventState.Draft;
        _context.Events.Add(evt);
        _context.SaveChanges();
        return ApiResponse<Event>.Ok(evt, "Evento creado");
    }

    public ApiResponse<Event> Update(int id, Event evt)
    {
        var existing = _context.Events.Find(id);
        if (existing == null) return ApiResponse<Event>.Fail("Evento no encontrado");
        if (existing.State == EventState.Finished)
            return ApiResponse<Event>.Fail("El evento está finalizado y no permite cambios.");

        existing.Title = evt.Title;
        existing.Description = evt.Description;
        existing.ScheduledStartDate = evt.ScheduledStartDate;
        existing.AllowUninvited = evt.AllowUninvited;
        existing.AllowExternal = evt.AllowExternal;
        existing.AutoStart = evt.AutoStart;

        _context.SaveChanges();
        return ApiResponse<Event>.Ok(existing, "Evento actualizado");
    }

    public ApiResponse<bool> StartEvent(int id)
    {
        var evt = _context.Events.Find(id);
        if (evt == null) return ApiResponse<bool>.Fail("Evento no encontrado");
        if (evt.State == EventState.Finished)
            return ApiResponse<bool>.Fail("El evento está finalizado.");
        if (evt.State == EventState.InProgress)
            return ApiResponse<bool>.Fail("El evento ya está en curso.");

        evt.State = EventState.InProgress;
        _context.SaveChanges();
        return ApiResponse<bool>.Ok(true, "Evento iniciado");
    }

    public ApiResponse<bool> FinishEvent(int id)
    {
        var evt = _context.Events.Find(id);
        if (evt == null) return ApiResponse<bool>.Fail("Evento no encontrado");
        if (evt.State != EventState.InProgress)
            return ApiResponse<bool>.Fail("Solo se puede finalizar un evento en curso.");

        evt.State = EventState.Finished;
        evt.ActualEndDate = DateTime.Now;
        _context.SaveChanges();
        return ApiResponse<bool>.Ok(true, "Evento finalizado");
    }

    public ApiResponse<Invitation> InvitePerson(int eventId, int personId)
    {
        var evt = _context.Events.Find(eventId);
        if (evt == null)
            return ApiResponse<Invitation>.Fail("Evento no existe");
        if (evt.State == EventState.Finished)
            return ApiResponse<Invitation>.Fail("El evento está finalizado.");
        
        if (!_context.People.Any(p => p.Id == personId))
            return ApiResponse<Invitation>.Fail("Persona no existe");

        if (_context.Invitations.Any(i => i.EventId == eventId && i.PersonId == personId))
            return ApiResponse<Invitation>.Fail("Ya está invitado");

        var inv = new Invitation { EventId = eventId, PersonId = personId };
        _context.Invitations.Add(inv);
        _context.SaveChanges();
        
        return ApiResponse<Invitation>.Ok(inv, "Invitación creada");
    }

    public ApiResponse<int> InviteGroup(int eventId, int groupId)
    {
        var evt = _context.Events.Find(eventId);
        if (evt == null)
            return ApiResponse<int>.Fail("Evento no existe");
        if (evt.State == EventState.Finished)
            return ApiResponse<int>.Fail("El evento está finalizado.");

        var members = _context.PersonGroups
            .Where(pg => pg.GroupId == groupId)
            .Select(pg => pg.PersonId)
            .ToList();

        if (!members.Any()) return ApiResponse<int>.Fail("El grupo está vacío");

        var existing = _context.Invitations
            .Where(i => i.EventId == eventId)
            .Select(i => i.PersonId)
            .ToList();

        var toAdd = members.Except(existing).ToList();
        
        if (!toAdd.Any()) return ApiResponse<int>.Ok(0, "Todos ya estaban invitados");

        var invitations = toAdd.Select(pid => new Invitation { EventId = eventId, PersonId = pid });
        _context.Invitations.AddRange(invitations);
        _context.SaveChanges();

        return ApiResponse<int>.Ok(toAdd.Count, $"Se agregaron {toAdd.Count} invitaciones");
    }

    public ApiResponse<bool> RemoveInvitation(int eventId, int personId)
    {
        var evt = _context.Events.Find(eventId);
        if (evt == null) return ApiResponse<bool>.Fail("Evento no existe");
        if (evt.State == EventState.Finished)
            return ApiResponse<bool>.Fail("El evento está finalizado.");

        var inv = _context.Invitations
            .FirstOrDefault(i => i.EventId == eventId && i.PersonId == personId);
        
        if (inv == null) return ApiResponse<bool>.Fail("Invitación no encontrada");

        _context.Invitations.Remove(inv);
        _context.SaveChanges();
        return ApiResponse<bool>.Ok(true, "Invitación eliminada");
    }

    public ApiResponse<bool> Delete(int id)
    {
        var evt = _context.Events.Find(id);
        if (evt == null) return ApiResponse<bool>.Fail("Evento no encontrado");

        var invitations = _context.Invitations.Where(i => i.EventId == id).ToList();
        var assistances = _context.Assistances.Where(a => a.EventId == id).ToList();

        if (invitations.Any())
        {
            _context.Invitations.RemoveRange(invitations);
        }

        if (assistances.Any())
        {
            _context.Assistances.RemoveRange(assistances);
        }

        _context.Events.Remove(evt);
        _context.SaveChanges();
        return ApiResponse<bool>.Ok(true, "Evento eliminado");
    }
}
