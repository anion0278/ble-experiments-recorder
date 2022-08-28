﻿using System.Threading.Tasks;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.Models.TestSubject;
using Microsoft.EntityFrameworkCore;

namespace Mebster.Myodam.UI.WPF.Data.Repositories;

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
            .Include(ts => ts.CustomizedAdjustments)
            .Include(ts => ts.CustomizedParameters)
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