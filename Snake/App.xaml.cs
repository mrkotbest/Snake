using Microsoft.Extensions.DependencyInjection;
using Snake.MVVM.ViewModels;
using Snake.MVVM.Views;
using Snake.Services;
using System.Windows;

namespace Snake
{
	public partial class App : Application
	{
		private readonly ServiceProvider _serviceProvider;

		public App()
        {
			IServiceCollection services = new ServiceCollection();

			services.AddSingleton(provider => new MainWindow { DataContext = provider.GetRequiredService<MainWindowViewModel>() });
			services.AddSingleton(provider => new MenuView { DataContext = provider.GetRequiredService<MenuViewModel>() });
			services.AddSingleton(provider => new GameView { DataContext = provider.GetRequiredService<GameViewModel>() });
			services.AddSingleton(provider => new SettingsView { DataContext = provider.GetRequiredService<SettingsViewModel>() });

			services.AddSingleton<MainWindowViewModel>();
			services.AddSingleton<MenuViewModel>();
			services.AddSingleton<GameViewModel>();
			services.AddSingleton<SettingsViewModel>();

			services.AddSingleton<INavigationService, NavigationService>();
			services.AddSingleton<Func<Type, ViewModel>>(provider => viewModelType => (ViewModel)provider.GetRequiredService(viewModelType));

			_serviceProvider = services.BuildServiceProvider();
        }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

			// Устанавливаем MenuViewModel как текущую ViewModel
			((MainWindowViewModel)mainWindow.DataContext).NavigationService.NavigateTo<MenuViewModel>();
			mainWindow.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_serviceProvider.Dispose();

			base.OnExit(e);
		}
	}
}