using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Mebster.Myodam.UI.WPF.ViewModels
{

    public class ViewModelBase : ObservableValidator
    {
        private SynchronizationContext ViewSynchronizationContext { get; }

        public ViewModelBase()
        {
            ViewSynchronizationContext = SynchronizationContext.Current!;
        }

        public void RunInViewContext(Action action)
        {
            ViewSynchronizationContext.Send(_ => action.Invoke(), null);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var value = GetType().GetProperty(e.PropertyName!)!.GetValue(this);
            ValidateProperty(value, e.PropertyName);
            base.OnPropertyChanged(e);
        }
    }
}
