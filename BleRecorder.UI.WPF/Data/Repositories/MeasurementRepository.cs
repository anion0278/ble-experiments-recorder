using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleRecorder.DataAccess;
using BleRecorder.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace BleRecorder.UI.WPF.Data.Repositories
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

    public void StartTrackingTestSubject(TestSubject testSubject)
    {
        Context.TestSubjects.Attach(testSubject);
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
