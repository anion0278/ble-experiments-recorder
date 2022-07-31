﻿using System.Drawing;
using BleRecorder.Business.Device;
using BleRecorder.Models.Device;

namespace BleRecorder.UI.WPF.ViewModels;

public class MechanismParametersViewModel : ViewModelBase
{
    public MechanismParameters Model { get; }

    public int AnkleAxisX
    {
        get => Model.AnkleAxisX.Value;
        set => Model.AnkleAxisX.Value = value;
    }

    public int AnkleAxisY
    {
        get => Model.AnkleAxisY.Value;
        set => Model.AnkleAxisY.Value = value;
    }

    public int AnkleAxisZ
    {
        get => Model.AnkleAxisZ.Value;
        set => Model.AnkleAxisZ.Value = value;
    }

    public int TibiaLength
    {
        get => Model.TibiaLength.Value;
        set => Model.TibiaLength.Value = value;
    }

    public int KneeAxisDeviation
    {
        get => Model.KneeAxisDeviation.Value;
        set => Model.KneeAxisDeviation.Value = value;
    }

    /// <summary>
    /// Design-time view-model
    /// </summary>
    public MechanismParametersViewModel()
    {
        Model = new MechanismParameters(new DeviceMechanicalAdjustments());
    }

    public MechanismParametersViewModel(MechanismParameters model)
    {
        Model = model;
    }
}