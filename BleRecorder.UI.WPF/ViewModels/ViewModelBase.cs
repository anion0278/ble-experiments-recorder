using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace BleRecorder.UI.WPF.ViewModels
{

  public class ViewModelBase : ObservableValidator
  {
      /// <summary>
      /// Returns actually new CV instead of default one - allows to separate filtering for different representations
      /// </summary>
      /// <typeparam name="T">Type of collection element</typeparam>
      /// <param name="collection">Target collection</param>
      /// <returns>New instance of Collection View of the target collection</returns>
      protected static ICollectionView GetNewCollectionViewInstance<T>(IEnumerable<T> collection)
      {
          return new CollectionViewSource { Source = collection }.View;
      }
}
}
