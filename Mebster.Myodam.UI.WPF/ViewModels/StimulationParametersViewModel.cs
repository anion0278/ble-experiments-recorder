using System;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.Data.Repositories;

namespace Mebster.Myodam.UI.WPF.ViewModels;

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
    public int RestTimeSeconds
    {
        get => (int)_model.RestTime.TotalSeconds;
        set => _model.RestTime = TimeSpan.FromSeconds(value);
    }

    public int FatigueRepetitions
    {
        get => _model.FatigueRepetitions;
        set => _model.FatigueRepetitions = value;
    }

    // Design-time 
    public StimulationParametersViewModel()
    {
    }

    public StimulationParametersViewModel(StimulationParameters model)
    {
        _model = model;
    }
}