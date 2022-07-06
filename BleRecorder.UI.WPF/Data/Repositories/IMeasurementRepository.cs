using System.Collections.Generic;
using System.Threading.Tasks;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Data.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<Measurement>
  {
    void StartTrackingTestSubject(TestSubject testSubject);
    public Task<TestSubject?> GetTestSubjectById(int id);
  }
}