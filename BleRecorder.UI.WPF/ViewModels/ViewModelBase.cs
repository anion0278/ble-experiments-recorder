using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace BleRecorder.UI.WPF.ViewModels
{

    public class ViewModelBase : ObservableValidator
    {
        public SynchronizationContext ViewSynchronizationContext { get; }

        /// <summary>
        /// Returns actually new CV instead of default one - allows to separate filtering for different representations
        /// </summary>
        /// <typeparam name="T">Type of collection element</typeparam>
        /// <param name="collection">Target collection</param>
        /// <returns>New instance of Collection View of the target collection</returns>
        protected static ICollectionView GetNewCollectionViewInstance<T>(IEnumerable<T> collection)
        {
            return new CollectionViewSource { Source = collection}.View;
        }

        public ViewModelBase()
        {
            ViewSynchronizationContext = SynchronizationContext.Current!;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var value = GetType().GetProperty(e.PropertyName!)!.GetValue(this);
            ValidateProperty(value, e.PropertyName);
            base.OnPropertyChanged(e);
        }
    }
}
