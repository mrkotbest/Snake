using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.Services;

namespace Snake.MVVM.ViewModels
{
	public partial class SettingsViewModel(INavigationService navigationService) : ViewModel
	{
		[ObservableProperty]
		private INavigationService _navigationService = navigationService;

		[RelayCommand]
		private void NavigateToMenu() => NavigationService.NavigateTo<MenuViewModel>();
	}
}