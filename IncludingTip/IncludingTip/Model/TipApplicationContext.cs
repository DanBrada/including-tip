using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration;

namespace IncludingTip.Model;

public class TipApplicationContext(DbContextOptions<TipApplicationContext> options): DbContext(options)
{
    
    public DbSet<Place> Places { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Tip> Tips { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CountryEdit> CountryEdits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(b => b.UserId)
            .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();
        
        modelBuilder.Entity<Tip>()
            .Property(b => b.Id)
            .HasValueGenerator<NpgsqlSequentialGuidValueGenerator>();

        modelBuilder.Entity<Place>()
            .HasOne(e => e.Country)
            .WithMany(e => e.Places)
            .HasForeignKey(e => e.CountryCode);
    }

    public static string ConfigureConnectionFromEnv()
    {
        string host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        int port = Int32.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432");

        string user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        string password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "toor";

        return $"Username={user};Password={password};Host={host};Port={port}";
    }
}