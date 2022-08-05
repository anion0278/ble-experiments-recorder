using System.Collections.ObjectModel;
using Mebster.Myodam.Business.Exception;
using Mebster.Myodam.Models.Device;
using Mebster.Myodam.Models.TestSubject;
using Nito.AsyncEx;

namespace Mebster.Myodam.Business.Device;

public class Calibrator
{
    private MyodamDevice _device;
    private AsyncAutoResetEvent _requiredDatasetCollected;
    private ObservableCollection<MeasuredValue> _dataset = new();
    private int _datasetLengthForAverage = 10;
    private readonly CancellationTokenSource _cts = new();

    private void DatasetCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_dataset.Count == _datasetLengthForAverage) _requiredDatasetCollected.Set();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    /// <exception cref="DeviceCalibrationException">Device is not connected or is not measuring (not raising event on data received)./exception>
    /// <exception cref="TimeoutException">Canceled async waiting, due to timeout.</exception>
    public async Task<double> GetCalibrationValue(MyodamDevice device)
    {
        if (!device.IsCurrentlyMeasuring || !device.IsConnected) throw new DeviceCalibrationException();

        _device = device;
        _requiredDatasetCollected = new AsyncAutoResetEvent(false);
        _dataset.CollectionChanged += DatasetCollectionChanged;
        _device.NewValueReceived += CalibrationNewValueReceived;

        const double increasedInterval = 2.5;
        var timeout = TimeSpan.FromMilliseconds(_device.DataRequestInterval.TotalMilliseconds * (_datasetLengthForAverage * increasedInterval));
        _cts.CancelAfter(timeout);
        try
        {
            await _requiredDatasetCollected.WaitAsync(_cts.Token);
        }
        catch (TaskCanceledException exception)
        {
            throw new TimeoutException($"Could not collect data in allotted time: {timeout.TotalSeconds}s,", exception);
        }

        _device.NewValueReceived -= CalibrationNewValueReceived;
        _dataset.CollectionChanged -= DatasetCollectionChanged;

        return _dataset.Average(v => v.Value);
    }

    private void CalibrationNewValueReceived(object? sender, MeasuredValue e)
    {
        _dataset.Add(e);
    }
}