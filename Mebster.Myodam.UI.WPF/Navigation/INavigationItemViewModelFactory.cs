using Mebster.Myodam.Models.TestSubjects;

namespace Mebster.Myodam.UI.WPF.Navigation;

public interface INavigationItemViewModelFactory
{
    INavigationItemViewModel GetViewModel(TestSubject ts);
}