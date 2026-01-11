namespace AssistantApp.Shared.Models;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsCreatedAtRuntime { get; set; } = false; // Flag para saber si fue creado durante una toma de asistencia

    // Relación Muchos a Muchos con Grupos
    public List<PersonGroup> PersonGroups { get; set; } = new();

    // Relación con Eventos
    public List<Assistance> Assistances { get; set; } = new();
    public List<Invitation> Invitations { get; set; } = new();
}
