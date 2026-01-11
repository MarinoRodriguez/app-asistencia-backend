using AssistantApp.Shared;
using AssistantApp.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AssistantApp.API.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Tables
    public DbSet<Person> People { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<PersonGroup> PersonGroups { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Assistance> Assistances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the many-to-many relationship between Person and Group
        modelBuilder.Entity<PersonGroup>()
            .HasKey(pg => new { pg.PersonId, pg.GroupId });

        modelBuilder.Entity<PersonGroup>()
            .HasOne(pg => pg.Person)
            .WithMany(p => p.PersonGroups)
            .HasForeignKey(pg => pg.PersonId);

        modelBuilder.Entity<PersonGroup>()
            .HasOne(pg => pg.Group)
            .WithMany(g => g.PersonGroups)
            .HasForeignKey(pg => pg.GroupId);
    }
}