using System.Collections.Generic;
using System.Threading.Tasks;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Data.Lookups
{
  public interface IMeasurementLookupDataService
  {
    Task<List<LookupItem>> GetMeasurementLookupAsync();
  }
}
