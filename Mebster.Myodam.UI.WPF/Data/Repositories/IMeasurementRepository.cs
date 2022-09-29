using System.Collections.Generic;
using System.Threading.Tasks;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Data.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<MeasurementBase>
  {
    public Task<TestSubject?> GetTestSubjectById(int id);

    Task ReloadTestSubjectAsync(TestSubject testSubject);
  }
}