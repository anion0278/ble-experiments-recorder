using System.Collections.Generic;
using System.Threading.Tasks;
using BleRecorder.DataAccess;
using BleRecorder.Models.Device;

namespace BleRecorder.UI.WPF.Data.Repositories;

public class StimulationParametersRepository : GenericRepository<StimulationParameters, ExperimentsDbContext>, IStimulationParametersRepository
{
    public StimulationParametersRepository(ExperimentsDbContext context) : base(context)
    {
    }

}

public interface IStimulationParametersRepository
{
    void Add(StimulationParameters model);

    Task<StimulationParameters?> GetByIdAsync(int id);

    Task<IEnumerable<StimulationParameters>> GetAllAsync();

    Task SaveAsync();
}