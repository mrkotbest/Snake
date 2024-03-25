using Snake.MVVM.ViewModels;

namespace Snake.Services
{
	public interface INavigationService
	{
		void NavigateTo<TViewModel>() where TViewModel : ViewModel;
	}
}