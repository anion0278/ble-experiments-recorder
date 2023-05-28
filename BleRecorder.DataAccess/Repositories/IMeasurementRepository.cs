using BleRecorder.Models.Measurements;
using BleRecorder.Models.TestSubjects;

namespace BleRecorder.DataAccess.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<Measurement>
  {
    public Task<TestSubject?> GetTestSubjectById(int id);

    Task ReloadTestSubjectAsync(TestSubject testSubject);
  }
}