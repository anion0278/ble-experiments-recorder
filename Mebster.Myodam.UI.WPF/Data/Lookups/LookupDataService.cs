using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace Mebster.Myodam.UI.WPF.Data.Lookups
{
    public class LookupDataService : ITestSubjectLookupDataService, IMeasurementLookupDataService
    {
        private readonly Func<ExperimentsDbContext> _contextCreator;

        public LookupDataService(Func<ExperimentsDbContext> contextCreator)
        {
            _contextCreator = contextCreator;
        }

        public async Task<IEnumerable<LookupItem>> GetTestSubjectLookupAsync()
        {
            // TODO check why throws System.InvalidOperationException
            using (var ctx = _contextCreator())
            {
                return await ctx.TestSubjects.AsNoTracking().Select(f =>
                  new LookupItem
                  {
                      Id = f.Id,
                      DisplayMember = f.FirstName + " " + f.LastName
                  })
                  .ToListAsync();
            }
        }

        public async Task<List<LookupItem>> GetMeasurementLookupAsync()
        {
            using (var ctx = _contextCreator())
            {
                var items = await ctx.Measurements.AsNoTracking()
                  .Select(m =>
                     new LookupItem
                     {
                         Id = m.Id,
                         DisplayMember = m.Notes
                     })
                  .ToListAsync();
                return items;
            }
        }
    }
}
