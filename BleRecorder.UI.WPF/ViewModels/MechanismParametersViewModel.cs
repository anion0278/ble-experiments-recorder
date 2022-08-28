﻿using System.Drawing;
using AutoMapper;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;

namespace BleRecorder.UI.WPF.ViewModels;

public class MechanismParametersViewModel : ViewModelBase
{
    private readonly IMapper _mapper;
    public MechanismParameters Model { get; }

    public double FixtureAdductionAbductionAngle
    {
        get => Model.FixtureAdductionAbductionAngle.Value;
        set => Model.FixtureAdductionAbductionAngle.Value = value;
    }

    public double FixtureProximalDistalDistance
    {
        get => Model.FixtureProximalDistalDistance.Value;
        set => Model.FixtureProximalDistalDistance.Value = value;
    }

    public double FixtureAnteroPosteriorDistance
    {
        get => Model.FixtureAnteroPosteriorDistance.Value;
        set => Model.FixtureAnteroPosteriorDistance.Value = value;
    }

    public double CuffProximalDistalDistance
    {
        get => Model.CuffProximalDistalDistance.Value;
        set => Model.CuffProximalDistalDistance.Value = value;
    }

    /// <summary>
    /// Design-time view-model
    /// </summary>
    public MechanismParametersViewModel()
    {
        Model = new MechanismParameters(new DeviceMechanicalAdjustments());
    }

    public MechanismParametersViewModel(MechanismParameters model, IMapper mapper)
    {
        _mapper = mapper;
        Model = model;
    }

    public void CopyAdjustmentValuesTo(DeviceMechanicalAdjustments target)
    {
        // An alternative to mapping could have been a ParamValue type, which however has a disadvantage - it should be immutable VO, which makes it inappropriate
        _mapper.Map(Model.GetCurrentAdjustments(), target);
    }
}