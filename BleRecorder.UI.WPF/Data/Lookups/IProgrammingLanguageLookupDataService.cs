using System.Collections.Generic;
using System.Threading.Tasks;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Data.Lookups
{
  public interface IProgrammingLanguageLookupDataService
  {
    Task<IEnumerable<LookupItem>> GetProgrammingLanguageLookupAsync();
  }
}