namespace AssistantApp.Shared.Models;
public class Assistance
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public DateTime RegistrationDateTime { get; set; }

    // Enum: Presente, Ausente, Tarde, Justificado
    public AssistanceType Status { get; set; }

    // Auditoría: ¿Quién registró esto? (El Admin o un Staff)
    public int? RegisteredByUserId { get; set; }
}