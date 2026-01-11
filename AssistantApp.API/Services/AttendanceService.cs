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
            existing.Status = status; // CORREGIDO: Type -> Status
            existing.RegistrationDateTime = DateTime.Now; // CORREGIDO: DateTime -> RegistrationDateTime
            _context.SaveChanges();
            return ApiResponse<Assistance>.Ok(existing, "Asistencia actualizada");
        }

        // Crear nuevo registro
        var assistance = new Assistance
        {
            EventId = eventId,
            PersonId = personId,
            Status = status, // CORREGIDO
            RegistrationDateTime = DateTime.Now // CORREGIDO
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

            newPerson.IsCreatedAtRuntime = true; // CORREGIDO: IsExternal -> IsCreatedAtRuntime
            _context.People.Add(newPerson);
            _context.SaveChanges();

            // 2. Marcar Asistencia
            var assistance = new Assistance
            {
                EventId = eventId,
                PersonId = newPerson.Id,
                Status = status, // CORREGIDO
                RegistrationDateTime = DateTime.Now // CORREGIDO
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
}