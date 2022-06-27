using System.Diagnostics.Metrics;
using Mebster.Myodam.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace Mebster.Myodam.DataAccess;
public class ExperimentsDbContext : DbContext
{
    public DbSet<TestSubject> TestSubjects { get; set; }
    public DbSet<Measurement> Measurements { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Mebster.Myodam.Experiments.db");
        //optionsBuilder.UseLazyLoadingProxies(); // TODO investigate
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Measurement>()
            .Property(e => e.ForceData)
            .HasConversion(
                v => Measurement.ConvertForceValuesToJson(v),
                v => Measurement.ConvertInternalJsonToForceValues(v) ?? Array.Empty<MeasuredValue>());

        base.OnModelCreating(modelBuilder);
    }

}
