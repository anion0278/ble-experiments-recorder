using System.Collections.Generic;
using System.Threading.Tasks;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Data.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<Models.TestSubject.Measurement>
  {
    public Task<Models.TestSubject.TestSubject?> GetTestSubjectById(int id);

    Task ReloadTestSubjectAsync(Models.TestSubject.TestSubject testSubject);
  }
}