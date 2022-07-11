﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace Mebster.Myodam.UI.WPF.Data.Repositories;
public class MeasurementRepository : GenericRepository<Measurement, ExperimentsDbContext>, IMeasurementRepository
{
    public MeasurementRepository(ExperimentsDbContext context) : base(context)
    {
    }

    public override async Task<Measurement?> GetByIdAsync(int id)
    {
        return await Context.Measurements.Include(m => m.TestSubject)
          .SingleOrDefaultAsync(m => m.Id == id);
    }

    public async Task<TestSubject?> GetTestSubjectById(int id)
    {
        return await Context.TestSubjects.FindAsync(id);
    }

    public async Task ReloadTestSubjectAsync(TestSubject testSubject)
    {
        await Context.Entry(testSubject).ReloadAsync();
    }
}