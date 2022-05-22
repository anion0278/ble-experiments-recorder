using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace Mebster.Myodam.UI.WPF.Data.Repositories
{
  public class MeasurementRepository : GenericRepository<Measurement, ExperimentsDbContext>, 
    IMeasurementRepository
  {
    public MeasurementRepository(ExperimentsDbContext context) : base(context)
    {
    }

    public async override Task<Measurement> GetByIdAsync(int id)
    {
      return await Context.Measurements
        .Include(m => m.TestSubject)
        .SingleAsync(m => m.Id == id);
    }

    public async Task<List<TestSubject>> GetAllTestSubjectsAsync()
    {
      return await Context.Set<TestSubject>()
          .ToListAsync();
    }

    public async Task ReloadTestSubjectAsync(int testSubjectId)
    {
      var dbEntityEntry = Context.ChangeTracker.Entries<TestSubject>()
        .SingleOrDefault(db => db.Entity.Id == testSubjectId);
      if(dbEntityEntry!=null)
      {
        await dbEntityEntry.ReloadAsync();
      }
    }
  }
}
