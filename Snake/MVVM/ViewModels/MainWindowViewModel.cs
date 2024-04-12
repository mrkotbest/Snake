using CommunityToolkit.Mvvm.ComponentModel;
using Snake.Services;

namespace Snake.MVVM.ViewModels
{
	public partial class MainWindowViewModel : ViewModel
	{
        [ObservableProperty]
        private INavigationService _navigationService;

        public MainWindowViewModel(INavigationService navigationService)
			=> NavigationService = navigationService;
	}
}