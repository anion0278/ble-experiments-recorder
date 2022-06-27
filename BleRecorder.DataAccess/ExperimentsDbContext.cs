using System.Diagnostics.Metrics;
using BleRecorder.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.DataAccess;
public class ExperimentsDbContext : DbContext
{
    public DbSet<TestSubject> TestSubjects { get; set; }
    public DbSet<Measurement> Measurements { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=BleRecorder.Experiments.db");
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
