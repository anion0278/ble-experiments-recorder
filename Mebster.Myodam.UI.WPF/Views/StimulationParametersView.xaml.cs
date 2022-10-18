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

namespace Mebster.Myodam.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for StimulationParametersView.xaml
    /// </summary>
    public partial class StimulationParametersView : UserControl
    {
        public static readonly DependencyProperty IsFatigueParametersAdjustableProperty = DependencyProperty.Register(
            "IsFatigueParametersAdjustable", typeof(bool), typeof(StimulationParametersView), new PropertyMetadata(true));

        public bool IsFatigueParametersAdjustable
        {
            get { return (bool)GetValue(IsFatigueParametersAdjustableProperty); }
            set { SetValue(IsFatigueParametersAdjustableProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(StimulationParametersView), new PropertyMetadata("Stimulation parameters:"));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public StimulationParametersView()
        {
            InitializeComponent();
        }
    }
}
