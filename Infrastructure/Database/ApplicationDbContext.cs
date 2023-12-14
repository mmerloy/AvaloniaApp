using Domain;
using Domain.Defects;
using Domain.MethodConfigurations;
using Domain.MethodConfigurations.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public DbSet<UserProfile> UsersProfiles { get; set; }

    public DbSet<Defect> Defects { get; set; }

    public DbSet<MethodConfiguration> MethodConfigurations { get; set; }

    public DbSet<ImageEntity> ImageEntities { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> contextOptions) : base(contextOptions)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MethodConfiguration>()
            .HasDiscriminator<string>("method_config_type")
            .HasValue<RecursionMethodConfiguration>("recursion")
            .HasValue<InterpolationMethodConfiguration>("interpolation")
            .HasValue<WeightCoefficientsMethodConfiguration>("weighted");

        var options = new JsonSerializerOptions(JsonSerializerDefaults.General);

        modelBuilder
            .Entity<Defect>()
            .Property(d => d.Location)
            .HasColumnName("LocationData")
            .HasColumnType("BLOB")
            .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                s => JsonSerializer.Deserialize<DefectLocation>(s, options)!,
                ValueComparer.CreateDefault(typeof(DefectLocation), true)
            );

        //.HasConversion(
        //    v =>
        //    {
        //        MemoryStream buffer = new();
        //        BinaryFormatter formatter = new();
        //        formatter.Serialize(buffer, v);
        //        return buffer.ToArray();
        //    },
        //    s =>
        //    {
        //        using MemoryStream buffer = new(s);
        //        BinaryFormatter formatter = new();
        //        return (DefectLocation)formatter.Deserialize(buffer);
        //    },
        //    ValueComparer.CreateDefault(typeof(DefectLocation), true)
        //    );
    }
}
