using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.MVVM.Models;
using Snake.MVVM.Views;
using Snake.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Snake.MVVM.ViewModels;

public partial class GameViewModel : ViewModel
{
	private static readonly Dictionary<CellType, BitmapImage> _cellValueToImage = new()
	{
		{ CellType.Empty, ImageService.Empty },
		{ CellType.Snake, ImageService.Body },
		{ CellType.Food, ImageService.Food },
	};
	private static readonly Dictionary<Direction, int> _directionToRotation = new()
	{
		{ Direction.Left, 270 },
		{ Direction.Up, 0 },
		{ Direction.Right, 90 },
		{ Direction.Down, 180 }
	};
	private static readonly Dictionary<(Direction, Direction), SnakeTurnType> _turnTypeByDirection = new()
	{
		{(Direction.Up, Direction.Right), SnakeTurnType.Clockwise},
		{(Direction.Right, Direction.Down), SnakeTurnType.Clockwise},
		{(Direction.Down, Direction.Left), SnakeTurnType.Clockwise},
		{(Direction.Left, Direction.Up), SnakeTurnType.Clockwise}
	};

	private readonly GameState _game;

	private readonly double _scoreMultiplier = 1;
	private double _speedMultiplier = 1;
	private int _speed = 100;
	private bool _isGameOver;
	private bool _isGameRunning;

	[ObservableProperty]
	private int _bestScore = 0;
	[ObservableProperty]
	private int _score = 0;
	[ObservableProperty]
	private string _speedStr = "1x";

	[ObservableProperty]
	private INavigationService _navigationService;
	[ObservableProperty]
	private Visibility _overlayVisibility;
	[ObservableProperty]
	private string _overlayText = "Press any key to start";

	public static byte Rows => 15;
	public static byte Columns => 15;

	public List<Image> GridImages { get; }

	[RelayCommand]
	private void NavigateToMenu()
		=> NavigationService.NavigateTo<MenuViewModel>();

	public GameViewModel(INavigationService navigationService, MainWindow main)
	{
		NavigationService = navigationService;

		GridImages = [];

		SetupGridWithEmptyCells(GridImages);

		_game = new GameState(Rows, Columns);

		_game.Snake.OnEatFood += HandleEatFood;
		_game.Snake.OnGameOver += HandleGameOver;

		main.PreviewKeyDown += OnPreviewKeyDown;
		main.KeyDown += OnKeyDown;
	}

	private void HandleEatFood()
	{
		Score++;
		_game.AddFood();
		UpdateSnakeSpeed();
	}
	private void HandleGameOver()
	{
		_isGameOver = true;
		BestScore = Score > BestScore ? Score : BestScore;
	}

	private void UpdateSnakeSpeed()
	{
		if (Score % _scoreMultiplier == 0)
		{
			_speed = (int)(_speed * 0.99); // Уменьшаем скорость на 1%
			SpeedStr = $"{_speedMultiplier += 0.1:F1}x"; // Увеличиваем коэффициент на 0.1 и обновляем строку
		}
	}

	private static void SetupGridWithEmptyCells(List<Image> gridImages)
	{
		for (int row = 0; row < Rows; row++)
		{
			for (int column = 0; column < Columns; column++)
			{
				Image image = new()
				{
					Source = ImageService.Empty,
					RenderTransformOrigin = new Point(0.5, 0.5)
				};
				gridImages.Add(image);
			}
		}
	}

	private async Task StartNewGame()
	{
		ReInitialize();
		Draw();
		await ShowCountDown();
		OverlayVisibility = Visibility.Hidden;
		await GameLoop();
		await ShowGameOver();
	}

	private void ReInitialize()
	{
		_game.Initialize();
		_speedMultiplier = 1;
		_speed = 100;
		_isGameOver = false;
		Score = 0;
		SpeedStr = "1x";
	}

	private void Draw()
	{
		DrawGrid();
		DrawSnake(_game.GetSnakeBody());
	}

	private void DrawGrid()
	{
		Image image;
		CellType cell;

		for (int row = 0; row < Rows; row++)
		{
			for (int column = 0; column < Columns; column++)
			{
				cell = _game.GetCurrentCell(row, column);
				// "row * Columns + column" это формула для преобразования двумерного индекса в одномерный.
				image = GridImages[row * Columns + column];
				image.Source = _cellValueToImage[cell];
				image.RenderTransform = Transform.Identity;
			}
		}
	}

	private static SnakeTurnType GetTurnType(Direction currentDirection, Direction previousDirection)
	{
		// Проверяем, является ли поворот по часовой стрелке или против
		if (_turnTypeByDirection.TryGetValue((previousDirection, currentDirection), out SnakeTurnType turnType))
			return turnType;
		return SnakeTurnType.CounterClockwise;
	}

	private void DrawSnake(LinkedList<SnakePart> snake, bool isDead = false)
	{
		Direction previousDirection = snake.Last?.Previous.Value.Direction ?? throw new InvalidOperationException("Snake must have at least two parts.");
		SnakePartType snakePartType;
		BitmapImage snakeImageSource;
		bool isTurning;

		foreach (SnakePart currentSnakePart in snake)
		{
			snakePartType = Models.Snake.GetSnakePartType(currentSnakePart);
			isTurning = currentSnakePart.Direction != previousDirection;

			snakeImageSource = Models.Snake.GetSnakeImageSource(snakePartType, isTurning, isDead);

			DrawSnakePart(currentSnakePart, previousDirection, snakeImageSource);

			previousDirection = currentSnakePart.Direction;
		}
	}
	private async Task DrawDeadSnake(LinkedList<SnakePart> snake)
	{
		Direction previousDirection = snake.Last?.Previous.Value.Direction ?? throw new InvalidOperationException("Snake must have at least two parts.");
		SnakePartType snakePartType;
		Image image;
		bool isTurning;

		foreach (SnakePart currentDeadSnakePart in snake)
		{
			snakePartType = Models.Snake.GetSnakePartType(currentDeadSnakePart);
			isTurning = currentDeadSnakePart.Direction != previousDirection;

			image = GridImages[currentDeadSnakePart.Position.Row * Columns + currentDeadSnakePart.Position.Column];

			image.Source = Models.Snake.GetSnakeImageSource(snakePartType, isTurning, isDead: true);
			await Task.Delay(50);

			previousDirection = currentDeadSnakePart.Direction;
		}
	}

	private void DrawSnakePart(SnakePart currentSnakePart, Direction previousDirection, BitmapImage snakePartImageSource)
	{
		Position currentPosition = currentSnakePart.Position;
		Direction currentDirection = currentSnakePart.Direction;

		int rotation = GetRotation(snakePartImageSource, currentDirection, previousDirection);

		SnakeTurnType turnType = GetTurnType(currentDirection, previousDirection);

		if (turnType == SnakeTurnType.Clockwise && snakePartImageSource == ImageService.Turn)
			rotation -= 270;

		Image image = GridImages[currentPosition.Row * Columns + currentPosition.Column];
		image.Source = snakePartImageSource;
		image.RenderTransform = new RotateTransform(rotation);
	}

	private static int GetRotation(BitmapImage snakeImageSource, Direction currentDirection, Direction previousDirection)
		=> snakeImageSource == ImageService.Tail ? _directionToRotation[previousDirection] : _directionToRotation[currentDirection];

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
		while (!_isGameOver)
		{
			_game.Move();
			Draw();
			await Task.Delay(_speed);
		}
	}

	private async Task ShowGameOver()
	{
		await DrawDeadSnake(_game.GetSnakeBody());
		await Task.Delay(1_000);
		OverlayVisibility = Visibility.Visible;
		OverlayText = "Press any key to start";
	}

	private async void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (OverlayVisibility == Visibility.Visible)
			e.Handled = true;

		if (!_isGameRunning)
		{
			_isGameRunning = true;
			await StartNewGame();
			_isGameRunning = false;
		}
	}
	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (_isGameOver)
			return;

		switch (e.Key)
		{
			case Key.A:
				_game.ChangeDirection(Direction.Left);
				break;
			case Key.W:
				_game.ChangeDirection(Direction.Up);
				break;
			case Key.D:
				_game.ChangeDirection(Direction.Right);
				break;
			case Key.S:
				_game.ChangeDirection(Direction.Down);
				break;
			default:
				break;
		}
	}
}