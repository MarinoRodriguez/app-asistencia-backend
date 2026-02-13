namespace AssistantApp.Shared.Models;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdNumber { get; set; }
    public string? PhotoUrl { get; set; }
    
    public bool IsActive { get; set; } = true; // Nuevo campo para Soft Delete
    public bool IsCreatedAtRuntime { get; set; } = false; 

    // Relación Muchos a Muchos con Grupos
    public List<PersonGroup> PersonGroups { get; set; } = new();

    // Relación con Eventos
    public List<Assistance> Assistances { get; set; } = new();
    public List<Invitation> Invitations { get; set; } = new();
}