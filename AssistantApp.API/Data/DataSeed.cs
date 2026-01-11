using AssistantApp.Shared.Models;

namespace AssistantApp.API.Data;

public static class DataSeed
{
    public static void SeedData(AppDbContext context)
    {
        // Si ya hay personas, no hacemos nada (para no duplicar datos)
        if (context.People.Any()) return;

        // 1. CREAR GRUPOS
        var grupoDev = new Group() { Name = "Equipo Desarrollo", Description = "Programadores y QA" };
        var grupoAdmin = new Group() { Name = "Administración", Description = "RRHH y Contabilidad" };

        context.Groups.AddRange(grupoDev, grupoAdmin);
        context.SaveChanges();

        // 2. CREAR PERSONAS
        var p1 = new Person()
        {
            Name = "Marino", LastName = "Admin", Email = "marino@test.com",
            PhotoUrl = "https://i.pravatar.cc/150?u=marino"
        };
        var p2 = new Person()
        {
            Name = "Juan", LastName = "Dev", Email = "juan@test.com", PhotoUrl = "https://i.pravatar.cc/150?u=juan"
        };
        var p3 = new Person() { Name = "Ana", LastName = "Guest", Email = "ana@externo.com", IsCreatedAtRuntime = true };

        context.People.AddRange(p1, p2, p3);
        context.SaveChanges();

        // 3. VINCULAR PERSONAS A GRUPOS (Relación N:M)
        context.PersonGroups.Add(new PersonGroup { PersonId = p1.Id, GroupId = grupoDev.Id });
        context.PersonGroups.Add(new PersonGroup { PersonId = p2.Id, GroupId = grupoDev.Id });
        context.PersonGroups.Add(new PersonGroup
            { PersonId = p1.Id, GroupId = grupoAdmin.Id }); // Marino está en ambos

        context.SaveChanges();

        // 4. CREAR EVENTOS DE PRUEBA

        // Evento A: YA INICIADO (Para probar tomar lista ahora mismo)
        var eventoActivo = new Event()
        {
            Title = "Daily Standup",
            ScheduledStartDate = DateTime.Now.AddHours(-1), // Empezó hace 1 hora
            State = EventState.InProgress,
            AllowUninvited = true,
            AllowExternal = false
        };

        // Evento B: FUTURO (Para probar el Auto-Inicio)
        var eventoFuturo = new Event
        {
            Title = "Fiesta Fin de Año",
            ScheduledStartDate = DateTime.Now.AddDays(10),
            State = EventState.Scheduled,
            AllowUninvited = true
        };

        context.Events.AddRange(eventoActivo, eventoFuturo);
        context.SaveChanges();

        // 5. CREAR INVITACIONES (Marino está invitado a la Daily)
        context.Invitations.Add(new Invitation() { EventId = eventoActivo.Id, PersonId = p1.Id });

        context.SaveChanges();
    }
}