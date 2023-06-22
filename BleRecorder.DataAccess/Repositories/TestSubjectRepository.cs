using BleRecorder.Models.Measurements;
using BleRecorder.Models.TestSubjects;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.DataAccess.Repositories;

public class TestSubjectRepository : GenericRepository<TestSubject, ExperimentsDbContext>, ITestSubjectRepository
{
    public TestSubjectRepository(ExperimentsDbContext context)
      : base(context)
    {
    }

    public override async Task<TestSubject?> GetByIdAsync(int testSubjectId)
    {
        return await Context.TestSubjects
            .Include(ts => ts.Measurements).ThenInclude(m => m.AdjustmentsDuringMeasurement)
            .Include(ts => ts.Measurements).ThenInclude(m => m.ParametersDuringMeasurement)
            .Include(ts => ts.CustomizedAdjustments)
            .Include(ts => ts.CustomizedParameters)
            .SingleOrDefaultAsync(s => s.Id == testSubjectId);
    }

    public async Task<TestSubject> ReloadAsync(TestSubject testSubject)
    {
        Context.Entry(testSubject).State = EntityState.Detached; // TODO replace with Reload
        return (await GetByIdAsync(testSubject.Id))!;
    }

    public async Task<IEnumerable<TestSubject>> GetAllWithRelatedDataAsync()
    {
        return await Context.TestSubjects
            .Include(ts => ts.Measurements).ThenInclude(m => m.AdjustmentsDuringMeasurement)
            .Include(ts => ts.Measurements).ThenInclude(m => m.ParametersDuringMeasurement)
            .Include(ts => ts.CustomizedAdjustments)
            .Include(ts => ts.CustomizedParameters)
            .ToListAsync();
    }
}
