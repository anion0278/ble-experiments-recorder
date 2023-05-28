using AutoMapper;
using Mebster.Myodam.Business.Device;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.UI.WPF.ViewModels;

namespace Mebster.Myodam.UI.WPF.Measurements;

public class MechanismParametersViewModel : ViewModelBase
{
    private readonly IMapper _mapper;
    public MechanismParameters Model { get; }

    public double FootplateAdductionAbductionAngle
    {
        get => Model.FootplateAdductionAbductionAngle.Value;
        set => Model.FootplateAdductionAbductionAngle.Value = value;
    }

    public double FootplateProximalDistalDistance
    {
        get => Model.FootplateProximalDistalDistance.Value;
        set => Model.FootplateProximalDistalDistance.Value = value;
    }

    public double FootplateAnteroPosteriorDistance
    {
        get => Model.FootplateAnteroPosteriorDistance.Value;
        set => Model.FootplateAnteroPosteriorDistance.Value = value;
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