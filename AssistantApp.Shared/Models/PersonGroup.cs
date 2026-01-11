namespace AssistantApp.Shared.Models;

public class PersonGroup
{
    public int PersonId { get; set; }
    public Person? Person { get; set; }

    public int GroupId { get; set; }
    public Group? Group { get; set; }
}