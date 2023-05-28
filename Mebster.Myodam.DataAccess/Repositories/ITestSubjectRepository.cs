using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.DataAccess.Repositories;

public interface ITestSubjectRepository : IGenericRepository<TestSubject>
{
    void RemoveMeasurement(Measurement measurementsCurrentItem);

    Task<TestSubject> ReloadAsync(TestSubject testSubject);
    Task<IEnumerable<TestSubject>> GetAllWithRelatedDataAsync();
}