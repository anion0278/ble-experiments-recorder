using System.Threading.Tasks;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace Mebster.Myodam.UI.WPF.Data.Repositories;

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
        // TODO check if Include is required
        return await Context.TestSubjects.Include(ts => ts.Measurements).SingleAsync(s => s.Id == testSubjectId);
    }
}
