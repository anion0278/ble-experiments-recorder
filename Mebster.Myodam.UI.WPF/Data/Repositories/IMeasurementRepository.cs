﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Data.Repositories
{
  public interface IMeasurementRepository : IGenericRepository<Measurement>
  {
    Task ReloadTestSubjectAsync(int testSubjectId);
    void StartTrackingTestSubject(TestSubject testSubject);
  }
}