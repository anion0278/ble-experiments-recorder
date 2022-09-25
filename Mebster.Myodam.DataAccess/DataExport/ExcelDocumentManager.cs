using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Mebster.Myodam.Models.TestSubject;
using System.Reflection;
using Castle.Core.Internal;
using System.Linq;
using Mebster.Myodam.Common.Extensions;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.DataAccess.DataExport;

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
                var orderedData = new object?[]
                {
                    measurement.Title,
                    measurement.Date != null ? measurement.Date : "[Not measured]",
                    measurement.Type.GetDescription(),
                    measurement.Notes,
                    measurement.MaxContractionLoad,
                    FormatMechanicalAdjustments(measurement.AdjustmentsDuringMeasurement),
                    FormatStimulationParameters(measurement.ParametersDuringMeasurement),
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

    private static string FormatStimulationParameters(StimulationParameters? stimulationParameters)
    {
        if (stimulationParameters == null) return string.Empty;

        return $"Frequency: {stimulationParameters.Frequency}Hz\n" +
               $"Pulse width: {stimulationParameters.PulseWidth}us\n" +
               $"Max. current limit: {stimulationParameters.Current}mA\n" +
               $"Stimulation time: {stimulationParameters.StimulationTime.TotalSeconds}s";
    }

    private static string FormatMechanicalAdjustments(DeviceMechanicalAdjustments? mechanicalAdjustments)
    {
        if (mechanicalAdjustments == null) return string.Empty;

        return $"C-Cuff proximal-distal: {mechanicalAdjustments.CuffProximalDistalDistance}\n" +
               $"Footplate adduction-abduction: {mechanicalAdjustments.FootplateAdductionAbductionAngle}\n" +
               $"Footplate antero-posterior: {mechanicalAdjustments.FootplateAnteroPosteriorDistance}\n" +
               $"Footplate proximal-distal: {mechanicalAdjustments.FootplateProximalDistalDistance}";
    }
}