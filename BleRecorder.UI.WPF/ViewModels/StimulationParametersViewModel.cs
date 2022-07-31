using System;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.Data.Repositories;

namespace BleRecorder.UI.WPF.ViewModels;

public class StimulationParametersViewModel : ViewModelBase
{
    private readonly StimulationParameters _model;

    public int CurrentMilliAmps
    {
        get => _model.Current;
        set => _model.Current = value;
    }

    public int FrequencyHz
    {
        get => _model.Frequency;
        set => _model.Frequency = value;
    }

    public StimulationPulseWidth PulseWidthMicroSeconds
    {
        get => _model.PulseWidth;
        set => _model.PulseWidth = value;
    }

    public int StimulationTimeSeconds
    {
        get => (int)_model.StimulationTime.TotalSeconds;
        set => _model.StimulationTime = TimeSpan.FromSeconds(value);
    }

    public StimulationParametersViewModel(StimulationParameters model)
    {
        _model = model;
    }
}