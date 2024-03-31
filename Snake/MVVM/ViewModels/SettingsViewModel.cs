using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.Services;

namespace Snake.MVVM.ViewModels
{
	public partial class SettingsViewModel : ViewModel
	{
		[ObservableProperty]
		private INavigationService _navigationService;

        public SettingsViewModel(INavigationService navigationService)
        {
			NavigationService = navigationService;
        }

        [RelayCommand]
		private void NavigateToMenu() => NavigationService.NavigateTo<MenuViewModel>();
	}
}