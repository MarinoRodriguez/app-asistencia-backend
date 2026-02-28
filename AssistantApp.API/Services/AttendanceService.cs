using AssistantApp.API.Data;
using AssistantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AssistantApp.API.Services;

public class AttendanceService
{
    private readonly AppDbContext _context;

    public AttendanceService(AppDbContext context)
    {
        _context = context;
    }

    // Obtener lista de asistencia de un evento
    public ApiResponse<List<Assistance>> GetByEvent(int eventId)
    {
        var list = _context.Assistances
            .Where(a => a.EventId == eventId)
            .Include(a => a.Person)
            .ToList();

        return ApiResponse<List<Assistance>>.Ok(list);
    }

    public ApiResponse<List<AttendanceRosterItem>> GetRoster(int eventId)
    {
        var evt = _context.Events.Find(eventId);
        if (evt == null) return ApiResponse<List<AttendanceRosterItem>>.Fail("Evento no encontrado");

        var invitedIds = _context.Invitations
            .Where(i => i.EventId == eventId)
            .Select(i => i.PersonId)
            .ToHashSet();

        var assistanceMap = _context.Assistances
            .Where(a => a.EventId == eventId)
            .ToDictionary(a => a.PersonId, a => a.Status);

        var peopleQuery = _context.People
            .Include(p => p.PersonGroups)
            .ThenInclude(pg => pg.Group)
            .AsQueryable();
        if (!evt.AllowUninvited)
        {
            peopleQuery = peopleQuery.Where(p => invitedIds.Contains(p.Id));
        }

        var people = peopleQuery.ToList();

        var roster = people.Select(person =>
        {
            var invited = invitedIds.Contains(person.Id);
            var hasRecord = assistanceMap.TryGetValue(person.Id, out var s);
            var status = hasRecord ? s : AssistanceType.Absent;
            return new AttendanceRosterItem
            {
                PersonId = person.Id,
                Invited = invited,
                Status = status,
                HasRecord = hasRecord,
                Person = new PersonSummary
                {
                    Id = person.Id,
                    Name = person.Name,
                    LastName = person.LastName,
                    Email = person.Email,
                    PhotoUrl = person.PhotoUrl,
                    Groups = person.PersonGroups
                        .Where(pg => pg.Group != null)
                        .Select(pg => new GroupSummary
                        {
                            Id = pg.Group!.Id,
                            Name = pg.Group!.Name
                        })
                        .ToList()
                }
            };
        })
        .OrderByDescending(item => item.Invited)
        .ThenBy(item => item.Person.Name)
        .ThenBy(item => item.Person.LastName)
        .ToList();

        return ApiResponse<List<AttendanceRosterItem>>.Ok(roster);
    }

    // Registrar Asistencia (Core)
    public ApiResponse<Assistance> MarkAttendance(int eventId, int personId, AssistanceType status)
    {
        var evt = _context.Events.Find(eventId);
        if (evt == null) return ApiResponse<Assistance>.Fail("Evento no encontrado");

        // Validar estado del evento
        if (evt.State != EventState.InProgress)
            return ApiResponse<Assistance>.Fail($"El evento no está en curso (Estado: {evt.State})");

        var person = _context.People.Find(personId);
        if (person == null) return ApiResponse<Assistance>.Fail("Persona no encontrada");

        // Validar Invitación vs Reglas
        bool isInvited = _context.Invitations.Any(i => i.EventId == eventId && i.PersonId == personId);

        if (!isInvited && !evt.AllowUninvited)
        {
            return ApiResponse<Assistance>.Fail("La persona no está invitada y el evento no admite no invitados.");
        }

        // Buscar si ya existe registro (para actualizar en vez de duplicar)
        var existing = _context.Assistances
            .FirstOrDefault(a => a.EventId == eventId && a.PersonId == personId);

        if (existing != null)
        {
            existing.Status = status; 
            existing.RegistrationDateTime = DateTime.Now; 
            _context.SaveChanges();
            return ApiResponse<Assistance>.Ok(existing, "Asistencia actualizada");
        }

        // Crear nuevo registro
        var assistance = new Assistance
        {
            EventId = eventId,
            PersonId = personId,
            Status = status, 
            RegistrationDateTime = DateTime.Now 
        };

        _context.Assistances.Add(assistance);
        _context.SaveChanges();

        return ApiResponse<Assistance>.Ok(assistance, "Asistencia registrada");
    }

    // Registrar Externo (Crear Persona + Marcar Asistencia en una transacción)
    public ApiResponse<Assistance> RegisterExternal(int eventId, Person newPerson, AssistanceType status)
    {
        var evt = _context.Events.Find(eventId);
        if (evt == null) return ApiResponse<Assistance>.Fail("Evento no encontrado");

        if (!evt.AllowExternal)
            return ApiResponse<Assistance>.Fail("Este evento no permite registrar externos.");

        using var transaction = _context.Database.BeginTransaction();
        try
        {
            // 1. Crear Persona
            if (!string.IsNullOrEmpty(newPerson.Email) && _context.People.Any(p => p.Email == newPerson.Email))
                return ApiResponse<Assistance>.Fail("Ya existe una persona con ese email.");

            newPerson.IsCreatedAtRuntime = true; 
            _context.People.Add(newPerson);
            _context.SaveChanges();

            // 2. Marcar Asistencia
            var assistance = new Assistance
            {
                EventId = eventId,
                PersonId = newPerson.Id,
                Status = status, 
                RegistrationDateTime = DateTime.Now 
            };

            _context.Assistances.Add(assistance);
            _context.SaveChanges();

            transaction.Commit();
            return ApiResponse<Assistance>.Ok(assistance, "Externo registrado y asistencia marcada");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return ApiResponse<Assistance>.Fail($"Error al registrar externo: {ex.Message}");
        }
    }

    // Eliminar Asistencia (Deshacer)
    public ApiResponse<bool> RemoveAttendance(int eventId, int personId)
    {
        var evt = _context.Events.Find(eventId);
        if (evt == null) return ApiResponse<bool>.Fail("Evento no encontrado");

        if (evt.State != EventState.InProgress)
            return ApiResponse<bool>.Fail("Solo se puede modificar asistencia en eventos en curso.");

        var assistance = _context.Assistances
            .FirstOrDefault(a => a.EventId == eventId && a.PersonId == personId);

        if (assistance == null) return ApiResponse<bool>.Fail("No hay registro de asistencia para eliminar.");

        _context.Assistances.Remove(assistance);
        _context.SaveChanges();

        return ApiResponse<bool>.Ok(true, "Asistencia eliminada");
    }
}

public class AttendanceRosterItem
{
    public int PersonId { get; set; }
    public bool Invited { get; set; }
    public AssistanceType Status { get; set; }
    public bool HasRecord { get; set; }
    public PersonSummary Person { get; set; } = new();
}

public class PersonSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }
    public List<GroupSummary> Groups { get; set; } = new();
}

public class GroupSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
