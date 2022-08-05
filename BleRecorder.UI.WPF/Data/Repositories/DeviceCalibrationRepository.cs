using System.Collections.Generic;
using System.Threading.Tasks;
using BleRecorder.DataAccess;
using BleRecorder.Models.Device;

namespace BleRecorder.UI.WPF.Data.Repositories;

public class DeviceCalibrationRepository : GenericRepository<DeviceCalibration, ExperimentsDbContext>, IDeviceCalibrationRepository
{
    public DeviceCalibrationRepository(ExperimentsDbContext context) : base(context)
    {
    }

}

public interface IDeviceCalibrationRepository
{
    void Add(DeviceCalibration model);

    Task<DeviceCalibration?> GetByIdAsync(int id);

    Task<IEnumerable<DeviceCalibration>> GetAllAsync();

    Task SaveAsync();
}