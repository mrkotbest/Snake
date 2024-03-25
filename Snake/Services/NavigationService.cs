using CommunityToolkit.Mvvm.ComponentModel;
using Snake.MVVM.ViewModels;

namespace Snake.Services
{
	public partial class NavigationService(Func<Type, ViewModel> viewModelFactory) : ObservableObject, INavigationService
	{
		[ObservableProperty]
		private ViewModel _currentViewModel;

		private readonly Func<Type, ViewModel> _viewModelFactory = viewModelFactory;

		public void NavigateTo<TViewModel>() where TViewModel : ViewModel
		{
			CurrentViewModel = _viewModelFactory?.Invoke(typeof(TViewModel));
		}
	}
}