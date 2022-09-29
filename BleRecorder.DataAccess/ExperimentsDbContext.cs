using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using BleRecorder.Models.Device;
using BleRecorder.Models.TestSubject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BleRecorder.DataAccess;
public sealed class ExperimentsDbContext : DbContext
{
    public DbSet<TestSubject> TestSubjects { get; set; }
    public DbSet<MeasurementBase> Measurements { get; set; }
    public DbSet<MaximumContractionMeasurement> MaximumContractionMeasurements { get; set; }
    public DbSet<IntermittentMeasurement> IntermittentMeasurements { get; set; }
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
            .Entity<MeasurementBase>()
            .HasDiscriminator<string>("MeasurementTypeDiscriminator");

        modelBuilder
            .Entity<IntermittentMeasurement>()
            .OwnsOne(m => m.MultiCycleRecord, 
                builder => SetupRecordConversion(builder));

        modelBuilder
            .Entity<MaximumContractionMeasurement>()
            .OwnsOne(m => m.Record, SetupRecordDataConversion);

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

    private static PropertyBuilder<ICollection<SingleContractionRecord>> SetupRecordConversion(OwnedNavigationBuilder<IntermittentMeasurement, MultipleContractionRecord> m)
    {
        return m.Property(p => p.Data).HasConversion(
            v => ConvertRecordsValuesToJson(v),
            v => ConvertInternalJson(v) ?? Array.Empty<SingleContractionRecord>());
    }

    private void SetupRecordDataConversion(OwnedNavigationBuilder<MaximumContractionMeasurement, SingleContractionRecord> builder)
    {
        builder.Property(p => p.Data).HasConversion(
            v => ConvertForceValuesToJson(v),
            v => ConvertInternalJsonToForceValues(v) ?? Array.Empty<MeasuredValue>());
    }

    public static ICollection<SingleContractionRecord>? ConvertInternalJson(string json)
    {
        return JsonSerializer.Deserialize<ICollection<SingleContractionRecord>?>(json);
    }

    public static string ConvertRecordsValuesToJson(ICollection<SingleContractionRecord> values)
    {
        return JsonSerializer.Serialize(values);
    }

    public static ICollection<MeasuredValue>? ConvertInternalJsonToForceValues(string json)
    {
        return JsonSerializer.Deserialize<ICollection<MeasuredValue>?>(json);
    }

    public static string ConvertForceValuesToJson(ICollection<MeasuredValue> values)
    {
        return JsonSerializer.Serialize(values);
    }

    private static void SetDataSeeding(ModelBuilder modelBuilder)
    {
        var defaultStimulationParameters = Models.Device.StimulationParameters.GetDefaultValues(1);
        modelBuilder.Entity<StimulationParameters>().HasData(defaultStimulationParameters);
    }
}
