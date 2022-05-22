using System;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Wrapper
{
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

    public DateTime DateFrom
    {
      get { return GetValue<DateTime>(); }
      set
      {
        SetValue(value);
        if (DateTo < DateFrom)
        {
          DateTo = DateFrom;
        }
      }
    }

    public DateTime DateTo
    {
      get { return GetValue<DateTime>(); }
      set
      {
        SetValue(value);
        if (DateTo < DateFrom)
        {
          DateFrom = DateTo;
        }
      }
    }
  }
}
