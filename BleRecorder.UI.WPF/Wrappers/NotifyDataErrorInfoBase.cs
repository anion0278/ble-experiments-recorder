using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BleRecorder.UI.WPF.ViewModels;

namespace BleRecorder.UI.WPF.Wrappers;

public abstract class NotifyDataErrorInfoBase : ViewModelBase  // TODO get rid, replace with ViewModelBase
{
    private Dictionary<string, List<string>> _errorsByPropertyName = new();

    public bool HasErrors => _errorsByPropertyName.Any();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public IEnumerable GetErrors(string propertyName)
    {
        return _errorsByPropertyName.ContainsKey(propertyName)
            ? _errorsByPropertyName[propertyName]
            : null;
    }

    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        base.OnPropertyChanged(nameof(HasErrors));
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errorsByPropertyName.ContainsKey(propertyName))
        {
            _errorsByPropertyName[propertyName] = new List<string>();
        }
        if (!_errorsByPropertyName[propertyName].Contains(error))
        {
            _errorsByPropertyName[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    protected void ClearErrors(string propertyName)
    {
        if (_errorsByPropertyName.ContainsKey(propertyName))
        {
            _errorsByPropertyName.Remove(propertyName);
            OnErrorsChanged(propertyName);
        }
    }
}