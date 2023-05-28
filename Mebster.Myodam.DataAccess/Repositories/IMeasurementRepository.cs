using Mebster.Myodam.Models.Measurements;
using Mebster.Myodam.Models.TestSubjects;

namespace Mebster.Myodam.DataAccess.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<Measurement>
  {
    public Task<TestSubject?> GetTestSubjectById(int id);

    Task ReloadTestSubjectAsync(TestSubject testSubject);
  }
}