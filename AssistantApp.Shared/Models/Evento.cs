namespace AssistantApp.Shared.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Tiempos
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; } // Cuándo se cerró el evento

    // Configuración (Tus reglas de negocio)
    public bool AllowUninvited { get; set; }
    public bool AllowExternal { get; set; }
    public bool AutoStart { get; set; }
    public EventState State { get; set; } = EventState.Draft;

    // Relaciones
    public List<Invitation> Invitations { get; set; } = new();
    public List<Assistance> Assistances { get; set; } = new();
}