using BleRecorder.Models.Measurements;
using BleRecorder.Models.TestSubjects;

namespace BleRecorder.DataAccess.Repositories;

public interface ITestSubjectRepository : IGenericRepository<TestSubject>
{
    Task<TestSubject> ReloadAsync(TestSubject testSubject);
    Task<IEnumerable<TestSubject>> GetAllWithRelatedDataAsync();
}