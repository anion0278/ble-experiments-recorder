using System;
using BleRecorder.Models.TestSubject;
using BleRecorder.UI.WPF.Wrappers;

namespace BleRecorder.UI.WPF.Wrappers;
public class MeasurementWrapper : ModelWrapper<Measurement>
{
    public MeasurementWrapper(Measurement model) : base(model)
    {
    }

    public int Id { get { return Model.Id; } }

    public string Title
    {
        get { return GetValue<string>(); }
        set { SetValue(value); }
    }

    public string Notes
    {
        get { return GetValue<string>(); }
        set { SetValue(value); }
    }

    public TestSubject TestSubject
    {
        get { return GetValue<TestSubject>(); }
        set { SetValue(value); }
    }
}
