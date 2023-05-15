using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Navigation;

public interface INavigationItemViewModelFactory
{
    INavigationItemViewModel GetViewModel(TestSubject ts);
}