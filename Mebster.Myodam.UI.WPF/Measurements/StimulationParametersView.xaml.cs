using System.Windows;
using System.Windows.Controls;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Measurements
{
    /// <summary>
    /// Interaction logic for StimulationParametersView.xaml
    /// </summary>
    public partial class StimulationParametersView : UserControl
    {
        public static readonly DependencyProperty MeasurementTypeProperty = DependencyProperty.Register(
            "MeasurementType", typeof(MeasurementType?), typeof(StimulationParametersView), new PropertyMetadata(null));

        public MeasurementType? MeasurementType
        {
            get { return (MeasurementType?)GetValue(MeasurementTypeProperty); }
            set { SetValue(MeasurementTypeProperty, value); }
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
