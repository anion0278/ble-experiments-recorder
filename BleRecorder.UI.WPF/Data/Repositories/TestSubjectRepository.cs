using System.Threading.Tasks;
using BleRecorder.DataAccess;
using BleRecorder.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.UI.WPF.Data.Repositories;

public interface ITestSubjectRepository : IGenericRepository<TestSubject>
{
    void RemoveMeasurement(Measurement measurementsCurrentItem);
}

public class TestSubjectRepository : GenericRepository<TestSubject, ExperimentsDbContext>, ITestSubjectRepository
{
    public TestSubjectRepository(ExperimentsDbContext context)
      : base(context)
    {
    }

    public override async Task<TestSubject> GetByIdAsync(int testSubjectId)
    {
        // TODO check if Include is required
        return await Context.TestSubjects.Include(ts => ts.Measurements).SingleAsync(s => s.Id == testSubjectId);
    }

    public void RemoveMeasurement(Measurement item) // TODO join repositories, or change Collection to List to remove item from TestSubj
    {
        Context.Measurements.Remove(item);
    }
}
