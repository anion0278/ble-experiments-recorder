using System.Collections.Generic;
using System.Threading.Tasks;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Data.Lookups
{
  public interface ITestSubjectLookupDataService
  {
    Task<IEnumerable<LookupItem>> GetTestSubjectLookupAsync();
  }
}