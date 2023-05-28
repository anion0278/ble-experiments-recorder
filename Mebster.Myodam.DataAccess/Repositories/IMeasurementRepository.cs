namespace Mebster.Myodam.DataAccess.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<Models.TestSubject.Measurement>
  {
    public Task<Models.TestSubject.TestSubject?> GetTestSubjectById(int id);

    Task ReloadTestSubjectAsync(Models.TestSubject.TestSubject testSubject);
  }
}