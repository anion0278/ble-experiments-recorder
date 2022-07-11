using System.Threading.Tasks;
using BleRecorder.DataAccess;
using BleRecorder.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.UI.WPF.Data.Repositories;

public interface ITestSubjectRepository : IGenericRepository<TestSubject>
{
    void RemoveMeasurement(Measurement measurementsCurrentItem);

    Task<TestSubject> ReloadAsync(TestSubject testSubject);
}

public class TestSubjectRepository : GenericRepository<TestSubject, ExperimentsDbContext>, ITestSubjectRepository
{
    public TestSubjectRepository(ExperimentsDbContext context)
      : base(context)
    {
    }

    public override async Task<TestSubject?> GetByIdAsync(int testSubjectId)
    {
        return await Context.TestSubjects
            .Include(ts => ts.Measurements)
            .SingleOrDefaultAsync(s => s.Id == testSubjectId);
    }

    public async Task<TestSubject> ReloadAsync(TestSubject testSubject)
    {
        Context.Entry(testSubject).State = EntityState.Detached;
        return (await GetByIdAsync(testSubject.Id))!;
    }

    public void RemoveMeasurement(Measurement item) // TODO join repositories, or change Collection to List to remove item from TestSubj
    {
        Context.Measurements.Remove(item);
    }
}
