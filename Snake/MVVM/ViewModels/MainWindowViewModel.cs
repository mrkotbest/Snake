using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.Services;

namespace Snake.MVVM.ViewModels
{
	public partial class MainWindowViewModel : ViewModel
	{
        [ObservableProperty]
        private INavigationService _navigationService;

        public MainWindowViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

		[RelayCommand]
        private void NavigateGame() => NavigationService.NavigateTo<GameViewModel>();

		[RelayCommand]
		private void NavigateSettings() => NavigationService.NavigateTo<SettingsViewModel>();
	}
}