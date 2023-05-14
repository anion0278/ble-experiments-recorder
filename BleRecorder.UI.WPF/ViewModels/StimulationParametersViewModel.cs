using System;
using BleRecorder.Business.Device;
using BleRecorder.Common;
using BleRecorder.Models.Device;
using BleRecorder.UI.WPF.Data.Repositories;

namespace BleRecorder.UI.WPF.ViewModels;

public class StimulationParametersViewModel : ViewModelBase
{
    private readonly IStimulationParameters _model;

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

    public int IntermittentStimulationTimeSeconds
    {
        get => (int)_model.IntermittentStimulationTime.TotalSeconds;
        set => _model.IntermittentStimulationTime = TimeSpan.FromSeconds(value);
    }

    public int RestTimeSeconds
    {
        get => (int)_model.RestTime.TotalSeconds;
        set
        {
            Guard.ValueShouldBeMoreOrEqualThan(value, 0);
            _model.RestTime = TimeSpan.FromSeconds(value);
        }
    }

    public int IntermittentRepetitions
    {
        get => _model.IntermittentRepetitions;
        set => _model.IntermittentRepetitions = value;
    }

    //// Design-time 
    //[Obsolete("Design-time only!")]
    //public StimulationParametersViewModel()
    //{
    //}

    public StimulationParametersViewModel(IStimulationParameters model)
    {
        _model = model;
    }
}