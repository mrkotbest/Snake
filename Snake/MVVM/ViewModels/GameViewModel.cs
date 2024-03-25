using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.MVVM.Models;
using Snake.MVVM.Views;
using Snake.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Snake.MVVM.ViewModels
{
	public partial class GameViewModel : ViewModel
	{
		private readonly GameState _gameState;

		private readonly Dictionary<CellValue, BitmapImage> _cellValueToImage = new()
		{
			{ CellValue.Empty, ImageService.Empty },
			{ CellValue.Snake, ImageService.Body },
			{ CellValue.Food, ImageService.Food },
		};

		private readonly Dictionary<Direction, int> _directionToRotation = new()
		{
			{ Direction.Left, 270 },
			{ Direction.Up, 0 },
			{ Direction.Right, 90 },
			{ Direction.Down, 180 }
		};

		private bool _gameRunning;

		[ObservableProperty]
		private INavigationService _navigationService;

		[ObservableProperty]
		private List<Image> _gridImages;

		[ObservableProperty]
		private Visibility _overlayVisibility;

		[ObservableProperty]
		private string _overlayText = "Press any key to start";

		[ObservableProperty]
		private string _speedDisplay = "1,00x";

		[ObservableProperty]
		private int _score;

		[ObservableProperty]
		private int _bestScore;

		public double GridWidth { get; private set; } = 400;
		public double GridHeight { get; private set; } = 400;
		public int Rows { get; } = 15;
		public int Columns { get; } = 15;

		public GameViewModel(MainWindow mainWindow, INavigationService navigationService)
		{
			NavigationService = navigationService;

			SetupGrid();

			_gameState = new GameState(Rows, Columns);

			mainWindow.PreviewKeyDown += OnPreviewKeyDown;
			mainWindow.KeyDown += OnKeyDown;
		}

		private void SetupGrid()
		{
			GridWidth = GridWidth != GridHeight ? GridHeight * (Columns / (double)Rows) : GridWidth;

			GridImages = new List<Image>();

			for (int row = 0; row < Rows; row++)
			{
				for (int column = 0; column < Columns; column++)
				{
					Image image = new()
					{
						Source = ImageService.Empty ?? new BitmapImage(),
						RenderTransformOrigin = new Point(0.5, 0.5)
					};
					GridImages.Add(image);
				}
			}
		}

		[RelayCommand]
		private void NavigateToMenu() => NavigationService.NavigateTo<MenuViewModel>();	

		private async Task StartNewGame()
		{
			_gameState.ReInitialize();
			Draw();
			UpdateProprties();
			await ShowCountDown();
			OverlayVisibility = Visibility.Hidden;
			await GameLoop();
			await ShowGameOver();
		}

		private void Draw()
		{
			DrawGrid();
			DrawSnake();
		}

		private void DrawGrid()
		{
			for (int row = 0; row < Rows; row++)
			{
				for (int column = 0; column < Columns; column++)
				{
					// "row * Columns + column" это формула для преобразования двумерного индекса в одномерный.
					CellValue cell = _gameState.GameBoard.Cells[row, column];
					GridImages[row * Columns + column].Source = _cellValueToImage[cell];
					GridImages[row * Columns + column].RenderTransform = Transform.Identity;
				}
			}
		}

		private void DrawSnake()
		{
			DrawSnakeHead();
			DrawSnakeTail();
		}
		private void DrawSnakeHead()
		{
			Position headPosition = _gameState.Snake.HeadPosition();
			int rotation = _directionToRotation[_gameState.Snake.CurrentDirection];

			Image image = GridImages[headPosition.Row * Columns + headPosition.Column];
			image.Source = ImageService.Head;
			image.RenderTransform = new RotateTransform(rotation);
		}
		private void DrawSnakeTail()
		{
			Direction tailDirection = _gameState.Snake.LastDirectionFromDirectionHistory();

			Position tailPosition = _gameState.Snake.TailPosition();
			int rotation = _directionToRotation[tailDirection.ReverseDirection()];

			Image image = GridImages[tailPosition.Row * Columns + tailPosition.Column];
			image.Source = ImageService.Tail;
			image.RenderTransform = new RotateTransform(rotation);
		}

		private void UpdateProprties()
		{
			SpeedDisplay = _gameState.SpeedDisplay;
			Score = _gameState.Score;
		}

		private async Task ShowCountDown()
		{
			for (int countDown = 3; countDown >= 1; countDown--)
			{
				OverlayText = countDown.ToString();
				await Task.Delay(500);
			}
		}

		private async Task GameLoop()
		{
			while (!_gameState.IsGameOver)
			{
				_gameState.Move();
				Draw();
				UpdateProprties();
				await Task.Delay(_gameState.Speed);
			}
		}

		private async Task ShowGameOver()
		{
			await DrawDeadSnake();
			await Task.Delay(1000);
			OverlayVisibility = Visibility.Visible;
			BestScore = _gameState.BestScore;
			OverlayText = "Press any key to start";
		}

		private async Task DrawDeadSnake()
		{
			Position position;
			Image image;
			List<Position> positions = new(_gameState.Snake.SnakeBody);

			for (int index = 0; index < positions.Count; index++)
			{
				position = positions[index];

				image = GridImages[position.Row * Columns + position.Column];
				image.Source = GetDeadImageSource(index, positions.Count);

				await Task.Delay(50);
			}
		}

		private static BitmapImage GetDeadImageSource(int index, int count)
		{
			if (index == 0) return ImageService.DeadHead;
			if (index == count - 1) return ImageService.DeadTail;
			return ImageService.DeadBody;
		}

		private async void OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (OverlayVisibility == Visibility.Visible)
				e.Handled = true;

			if (!_gameRunning)
			{
				_gameRunning = true;
				await StartNewGame();
				_gameRunning = false;
			}
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (_gameState.IsGameOver)
				return;

			switch (e.Key)
			{
				case Key.A:
					_gameState.Snake.ChangeDirection(Direction.Left);
					break;
				case Key.W:
					_gameState.Snake.ChangeDirection(Direction.Up);
					break;
				case Key.D:
					_gameState.Snake.ChangeDirection(Direction.Right);
					break;
				case Key.S:
					_gameState.Snake.ChangeDirection(Direction.Down);
					break;
				default:
					break;
			}
		}
	}
}