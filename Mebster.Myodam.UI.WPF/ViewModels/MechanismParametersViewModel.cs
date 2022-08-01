using System.Drawing;
using AutoMapper;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.UI.WPF.ViewModels;

public class MechanismParametersViewModel : ViewModelBase
{
    private readonly IMapper _mapper;
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