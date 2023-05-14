using System.Collections.Generic;
using System.Threading.Tasks;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Data.Repositories;

public interface ITestSubjectRepository : IGenericRepository<TestSubject>
{
    void RemoveMeasurement(Measurement measurementsCurrentItem);

    Task<TestSubject> ReloadAsync(TestSubject testSubject);
    Task<IEnumerable<TestSubject>> GetAllWithRelatedDataAsync();
}