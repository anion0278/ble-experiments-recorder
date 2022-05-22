using System.Collections.Generic;
using System.Threading.Tasks;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Data.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<Measurement>
  {
    Task<List<TestSubject>> GetAllTestSubjectsAsync();
    Task ReloadTestSubjectAsync(int testSubjectId);
  }
}