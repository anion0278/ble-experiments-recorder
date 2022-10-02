using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BleRecorder.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for StimulationParametersView.xaml
    /// </summary>
    public partial class StimulationParametersView : UserControl
    {
        public static readonly DependencyProperty IsIntermittentParametersAdjustableProperty = DependencyProperty.Register(
            "IsIntermittentParametersAdjustable", typeof(bool), typeof(StimulationParametersView), new PropertyMetadata(true));

        public bool IsIntermittentParametersAdjustable
        {
            get { return (bool)GetValue(IsIntermittentParametersAdjustableProperty); }
            set { SetValue(IsIntermittentParametersAdjustableProperty, value); }
        }

        public StimulationParametersView()
        {
            InitializeComponent();
        }
    }
}
