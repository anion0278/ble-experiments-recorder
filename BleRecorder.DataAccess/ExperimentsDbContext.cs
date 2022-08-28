using System.Diagnostics.Metrics;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.DataAccess;
public sealed class ExperimentsDbContext : DbContext
{
    public DbSet<TestSubject> TestSubjects { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<StimulationParameters> StimulationParameters { get; set; }

    public ExperimentsDbContext()
    {
        // swallowed System.InvalidOperationException and System.Reflection.TargetInvocationException is expected behaviour
        Database.Migrate();
    }

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

        modelBuilder
            .Entity<StimulationParameters>()
            .Property(e => e.PulseWidth)
            .HasConversion(
                p => p.Value,
                p => StimulationPulseWidth.AvailableOptions.SingleOrDefault(op => op.Value == p) 
                     ?? StimulationPulseWidth.AvailableOptions.First());

        SetDataSeeding(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void SetDataSeeding(ModelBuilder modelBuilder)
    {
        var defaultStimulationParameters = Models.Device.StimulationParameters.GetDefaultValues(1);
        modelBuilder.Entity<StimulationParameters>().HasData(defaultStimulationParameters);
    }
}
