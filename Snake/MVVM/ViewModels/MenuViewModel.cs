using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.Services;

namespace Snake.MVVM.ViewModels
{
	public partial class MenuViewModel(INavigationService navigationService) : ViewModel
    {
		[ObservableProperty]
		private INavigationService _navigationService = navigationService;

		[RelayCommand]
		private void NavigateGame()
			=> NavigationService.NavigateTo<GameViewModel>();

		[RelayCommand]
		private void NavigateSettings()
			=> NavigationService.NavigateTo<SettingsViewModel>();

		[RelayCommand]
		private static void Quit()
			=> System.Windows.Application.Current.Shutdown();
	}
}