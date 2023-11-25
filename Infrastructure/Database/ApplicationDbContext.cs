using Domain;
using Domain.MethodConfigurations;
using Domain.MethodConfigurations.Implementation;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public DbSet<UserProfile> UsersProfiles { get; set; }

    public DbSet<MethodConfiguration> MethodConfigurations { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> contextOptions) : base(contextOptions)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MethodConfiguration>()
            .HasDiscriminator<string>("method_config_type")
            .HasValue<RecursionMethodConfiguration>("recursion")
            .HasValue<InterpolationMethodConfiguration>("interpolation")
            .HasValue<WeightCoefficientsMethodConfiguration>("weighted");
    }
}
