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
    private int _datasetLengthForAverage = 20;

    private void DatasetCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_dataset.Count == _datasetLengthForAverage) _requiredDatasetCollected.Set();
    }

    public async Task<double> GetCalibrationValue(MyodamDevice device)
    {
        if (!device.IsCurrentlyMeasuring || !device.IsConnected) throw new DeviceCalibrationException();

        _device = device;
        _requiredDatasetCollected = new AsyncAutoResetEvent(false);
        _dataset.CollectionChanged += DatasetCollectionChanged;
        _device.NewValueReceived += CalibrationNewValueReceived;

        await _requiredDatasetCollected.WaitAsync(); // TODO timeout !!!

        _device.NewValueReceived -= CalibrationNewValueReceived;
        _dataset.CollectionChanged -= DatasetCollectionChanged;

        return _dataset.Average(v => v.Value);
    }

    private void CalibrationNewValueReceived(object? sender, MeasuredValue e)
    {
        _dataset.Add(e);
    }
}