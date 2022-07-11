using System.Collections.Generic;
using System.Threading.Tasks;
using Mebster.Myodam.DataAccess;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.UI.WPF.Data.Repositories;

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