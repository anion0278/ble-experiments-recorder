using System.Diagnostics;
using BleRecorder.Models.Device;
using BleRecorder.Models.Measurements;
using BleRecorder.Models.TestSubjects;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BleRecorder.DataAccess;
public sealed class ExperimentsDbContext : DbContext
{
    private readonly IConfigurationProvider _configProvider;
    public DbSet<TestSubject> TestSubjects { get; set; }
    public DbSet<Measurement> Measurements { get; set; }

    public ExperimentsDbContext(IConfigurationProvider configProvider)
    {
        _configProvider = configProvider;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _configProvider.TryGet("ConnectionString", out string connectionString);
#if DEBUG
        connectionString = @"Data Source=(localdb)\BleRecorderLocalDb;Initial Catalog=ExperimentsData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
#endif

        optionsBuilder.UseSqlServer(connectionString);
        //.LogTo(s => Debug.Print(s), new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information)
        //.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Measurement>()
            .Property(e => e.ContractionLoadData)
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
        var defaultStimulationParameters = StimulationParameters.GetDefaultValues(1);
        modelBuilder.Entity<StimulationParameters>().HasData(defaultStimulationParameters);
    }
}
