using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BleRecorder.UI.WPF.ViewModels
{

    public class ViewModelBase : ObservableValidator
    {
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var value = GetType().GetProperty(e.PropertyName!)!.GetValue(this);
            ValidateProperty(value, e.PropertyName);
            base.OnPropertyChanged(e);
        }

        protected ICollectionView GetDefaultCollectionView<T>(IEnumerable<T> collection)
        {
            return CollectionViewSource.GetDefaultView(collection);
        }
    }
}
