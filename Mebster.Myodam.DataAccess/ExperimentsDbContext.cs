using System.Diagnostics.Metrics;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace Mebster.Myodam.DataAccess;
public class ExperimentsDbContext : DbContext
{
    public DbSet<TestSubject> TestSubjects { get; set; }
    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<StimulationParameters> StimulationParameters { get; set; }
    public DbSet<DeviceCalibration> DeviceCalibrations { get; set; }

    public ExperimentsDbContext()
    {
        Database.Migrate();
    }

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
        var defaultCalibration = DeviceCalibration.GetDefaultValues(1);

        modelBuilder.Entity<StimulationParameters>().HasData(defaultStimulationParameters);
        modelBuilder.Entity<DeviceCalibration>().HasData(defaultCalibration);
    }
}
