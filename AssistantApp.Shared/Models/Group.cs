namespace AssistantApp.Shared.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Ej: "Departamento IT"
    public string? Description { get; set; }
    public bool Active { get; set; }
    // Relación Muchos a Muchos con Personas
    public List<PersonGroup> PersonGroups { get; set; } = new();
}