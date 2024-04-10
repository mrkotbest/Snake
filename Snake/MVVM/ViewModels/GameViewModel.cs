using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Snake.MVVM.Models;
using Snake.MVVM.Views;
using Snake.Services;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Image = System.Windows.Controls.Image;
using Snake = Snake.MVVM.Models.Snake;

namespace Snake.MVVM.ViewModels;

public partial class GameViewModel : ViewModel
{
	private static readonly Dictionary<CellType, BitmapImage> _cellValueToImage = new()
	{
		{ CellType.Empty, ImageService.SnakeImageSource["Empty"] },
		{ CellType.Snake, ImageService.SnakeImageSource["Body"] },
		{ CellType.Food, ImageService.SnakeImageSource["Food"] },
	};
	private static readonly Dictionary<Direction, int> _directionToRotation = new()
	{
		{ Direction.Left, 270 },
		{ Direction.Up, 0 },
		{ Direction.Right, 90 },
		{ Direction.Down, 180 }
	};

	private readonly GameState _game;

	private readonly double _scoreMultiplier = 2;
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
	private Key _pressedKey;

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

	[RelayCommand]
	private void KeyPressed(Key key)
		=> PressedKey = key;

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
				var image = new Image()
				{
					Source = ImageService.SnakeImageSource["Empty"],
					RenderTransformOrigin = new Point(0.5, 0.5)
				};
				gridImages.Add(image);
			}
		}
	}

	private async Task StartNewGame()
	{
		ReInitialize();
		await Draw();
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
		PressedKey = Key.None;
	}

	private async Task Draw()
	{
		DrawGrid();
		await DrawAliveOrDeadSnake(_game.GetSnakeBody());
	}

	private void DrawGrid()
	{
		CellType cell;
		Image image;

		for (int row = 0; row < Rows; row++)
		{
			for (int column = 0; column < Columns; column++)
			{
				cell = _game.GetCurrentCell(row, column);
				
				image = GridImages[row * Columns + column]; // row * Columns + column - это формула для преобразования двумерного индекса в одномерный.
				image.Source = _cellValueToImage[cell];
				image.RenderTransform = Transform.Identity;
			}
		}
	}

	private async Task DrawAliveOrDeadSnake(LinkedList<SnakePart> snake, bool isDead = false)
	{
		SnakePart previousPart = snake.Last?.Previous?.Value;
		foreach (SnakePart currentPart in snake)
		{
			DrawAliveOrDeadSnakePart(currentPart, previousPart, isDead);
			previousPart = currentPart;
			if (isDead)
			{
				await Task.Delay(60);
			}
		}
	}

	private void DrawAliveOrDeadSnakePart(SnakePart currentPart, SnakePart previousPart, bool isDead)
	{
		Position position = currentPart.Position;
		Direction direction = currentPart.Direction;
		bool isTurn = currentPart.IsTurn;

		SnakeTurnType turnType = Models.Snake.GetTurnType(direction, previousPart.Direction);
		SnakePartType partType = Models.Snake.GetSnakePartType(position);
		BitmapImage source = Models.Snake.GetSnakeImageSource(partType, isTurn, isDead);

		int rotation = GetRotation(direction, turnType, isTurn);

		if (partType == SnakePartType.Tail)
			rotation = _directionToRotation[previousPart.Direction];

		SetSnakePartImageSource(position, source, rotation);
	}

	private static int GetRotation(Direction direction, SnakeTurnType turnType, bool isTurn)
	{
		int rotation = _directionToRotation[direction];
		if (isTurn && turnType == SnakeTurnType.Clockwise)
			return rotation -= 270;
		return rotation;
	}

	private void SetSnakePartImageSource(Position position, BitmapImage source, int rotation)
	{
		Image image = GridImages[position.Row * Columns + position.Column];
		image.Source = source;
		image.RenderTransform = new RotateTransform(rotation);
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
		while (!_isGameOver)
		{
			_game.Move();
			await Draw();
			await Task.Delay(_speed);
		}
	}

	private async Task ShowGameOver()
	{
		await DrawAliveOrDeadSnake(_game.GetSnakeBody(), true);
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
		PressedKey = e.Key;
	}
}