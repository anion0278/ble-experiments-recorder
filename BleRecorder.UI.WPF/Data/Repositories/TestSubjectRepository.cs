using System.Threading.Tasks;
using BleRecorder.DataAccess;
using BleRecorder.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.UI.WPF.Data.Repositories;

public interface ITestSubjectRepository : IGenericRepository<TestSubject>
{
}

public class TestSubjectRepository : GenericRepository<TestSubject, ExperimentsDbContext>, ITestSubjectRepository
{
    public TestSubjectRepository(ExperimentsDbContext context)
      : base(context)
    {
    }

    public override async Task<TestSubject> GetByIdAsync(int testSubjectId)
    {
        return await Context.TestSubjects.Include(ts => ts.Measurements).SingleAsync(s => s.Id == testSubjectId);
    }
}
