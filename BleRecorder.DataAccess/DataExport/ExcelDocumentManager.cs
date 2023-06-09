﻿using ClosedXML.Excel;
using System.Reflection;
using System.Linq;
using BleRecorder.Common.Extensions;
using BleRecorder.Models.Device;
using BleRecorder.Models.Measurements;
using BleRecorder.Models.TestSubjects;

namespace BleRecorder.DataAccess.DataExport;

public interface IDocumentManager
{
    void Export(string path, IEnumerable<TestSubject> testSubjects);
}

public class ExcelDocumentManager : IDocumentManager
{
    public void Export(string path, IEnumerable<TestSubject> testSubjects)
    {
        //if (testSubjects.IsNullOrEmpty())

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(s => s.Contains("TestSubjectDataExportTemplate.xlsx"));
        var templateStream = assembly.GetManifestResourceStream(resourceName);

        var workbook = new XLWorkbook(templateStream);

        var baseWorksheet = workbook.Worksheet("TestSubject");
        foreach (var testSubject in testSubjects)
        {
            baseWorksheet = baseWorksheet.CopyTo(testSubject.FullName);
        }
        workbook.Worksheet("TestSubject").Delete();

        foreach (var testSubject in testSubjects)
        {
            var ws = workbook.Worksheet(testSubject.FullName);

            // TODO put into WS wrapper to encapsulate value setting logic
            ws.Range(nameof(testSubject.FirstName)).Value = testSubject.FirstName;
            ws.Range(nameof(testSubject.LastName)).Value = testSubject.LastName;
            ws.Range(nameof(testSubject.Notes)).Value = testSubject.Notes;
            ws.Range("MeasurementsCount").Value = testSubject.Measurements.Count;

            var table = ws.Table("MeasurementsTable");
            if (testSubject.Measurements.Count > 1)
            {
                table.InsertRowsBelow(testSubject.Measurements.Count - 1);
            }

            foreach (var (measurement, row) in testSubject.Measurements.OrderBy(m => m.Date)
                                                                        .Zip(table.Rows().Skip(1)))
            {
                var stimParams = measurement.ParametersDuringMeasurement;
                var orderedData = new []
                {
                    measurement.Title,
                    measurement.Date != null ? measurement.Date : "[Not measured]",
                    measurement.Type.GetDescription(),
                    measurement.Notes,
                    GetMeasurementStatistics(measurement),
                    FormatMechanicalAdjustments(measurement.AdjustmentsDuringMeasurement),
                    //FormatStimulationParameters(measurement.ParametersDuringMeasurement),
                    stimParams.Amplitude,
                    stimParams.Frequency,
                    stimParams.PulseWidth.Value,
                    FormatTime(measurement.Type == MeasurementType.MaximumContraction 
                        ? stimParams.StimulationTime
                        : stimParams.IntermittentStimulationTime),
                    FormatTime(stimParams.RestTime),
                    stimParams.IntermittentRepetitions,
                    measurement.SiteDuringMeasurement.GetDescription(),
                    measurement.PositionDuringMeasurement.GetDescription()
                };

                for (int i = 0; i < orderedData.Length; i++)
                {
                    row.Cell(i + 1).Value = orderedData[i];
                }
            }

        }
        workbook.SaveAs(path);
    }

    private string FormatTime(TimeSpan time)
    {
        return time.TotalSeconds.ToString();
    }

    private static object GetMeasurementStatistics(Measurement measurement)
    {
        switch (measurement.Type)
        {
            case MeasurementType.MaximumContraction:
                return measurement.MaxContractionLoad.ToString("0.00");
            case MeasurementType.Intermittent:
                return measurement.Intermittent.ToString();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static string FormatStimulationParameters(StimulationParameters? stimulationParameters)
    {
        if (stimulationParameters == null) return string.Empty;

        return $"Frequency: {stimulationParameters.Frequency}Hz\n" +
               $"Pulse width: {stimulationParameters.PulseWidth}us\n" +
               $"Max. amplitude limit: {stimulationParameters.Amplitude}mA\n" +
               $"Stimulation time: {stimulationParameters.StimulationTime.TotalSeconds}s";
    }

    private static string FormatMechanicalAdjustments(DeviceMechanicalAdjustments? mechanicalAdjustments)
    {
        if (mechanicalAdjustments == null) return string.Empty;

        return $"Cuff proximal-distal: {mechanicalAdjustments.CuffProximalDistalDistance}\n" +
               $"Fixture adduction-abduction: {mechanicalAdjustments.FixtureAdductionAbductionAngle}\n" +
               $"Fixture antero-posterior: {mechanicalAdjustments.FixtureAnteroPosteriorDistance}\n" +
               $"Fixture proximal-distal: {mechanicalAdjustments.FixtureProximalDistalDistance}";
    }
}