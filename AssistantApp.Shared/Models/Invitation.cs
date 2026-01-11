namespace AssistantApp.Shared.Models;

public class Invitation
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int PersonId { get; set; }
    public Person? Person { get; set; }
}