using System.Windows.Controls;

namespace Mebster.Myodam.UI.WPF.Navigation
{
    /// <summary>
    /// Interaction logic for NavigationView.xaml
    /// </summary>
    public partial class NavigationView : UserControl
    {
        public NavigationView()
        {
            InitializeComponent();
        }
        
        //TODO Can be solved with Attached Behavior
        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            UncheckedBox.IsChecked = false;
        }

        private void CheckedBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckedBox.IsChecked = true;
        }
    }
}
