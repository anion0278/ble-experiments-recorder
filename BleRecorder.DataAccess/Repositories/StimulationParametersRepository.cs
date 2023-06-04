using BleRecorder.Models.Device;

namespace BleRecorder.DataAccess.Repositories;

public class StimulationParametersRepository : GenericRepository<StimulationParameters, ExperimentsDbContext>, IStimulationParametersRepository
{
    public StimulationParametersRepository(ExperimentsDbContext context) : base(context)
    {
    }
}