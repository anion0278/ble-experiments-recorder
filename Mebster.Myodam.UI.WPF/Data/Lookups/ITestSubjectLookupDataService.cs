using System.Collections.Generic;
using System.Threading.Tasks;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Data.Lookups
{
  public interface ITestSubjectLookupDataService
  {
    Task<IEnumerable<LookupItem>> GetTestSubjectLookupAsync();
  }
}