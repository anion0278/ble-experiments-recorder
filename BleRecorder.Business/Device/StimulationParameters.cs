using System.Transactions;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.Business.Device;

public class StimulationParameters
{
    public float Current { get; } // TODO get rid of "primitive obsession" !
    public float Frequency { get; }
    public float PulseWidth { get; }

    public MeasurementType MeasurementType { get; }

    public StimulationParameters(float current, float frequency, float pulseWidth, MeasurementType measurementType)
    {
        Current = current;
        Frequency = frequency;
        PulseWidth = pulseWidth;
        MeasurementType = measurementType;
    }
}