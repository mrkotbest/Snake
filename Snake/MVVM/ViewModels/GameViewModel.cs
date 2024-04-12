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
using SnakeModel = Snake.MVVM.Models.Snake;

namespace Snake.MVVM.ViewModels;

public partial class GameViewModel : ViewModel
{
	private static readonly Dictionary<CellType, BitmapImage> _cellValueToImage = new()
	{
		{ CellType.Empty, ImageService.SnakeImageSources["Empty"] },
		{ CellType.Snake, ImageService.SnakeImageSources["Body"] },
		{ CellType.Food, ImageService.SnakeImageSources["Food"] },
	};
	private static readonly Dictionary<Direction, int> _directionToRotation = new()
	{
		{ Direction.Left, 270 },
		{ Direction.Up, 0 },
		{ Direction.Right, 90 },
		{ Direction.Down, 180 }
	};

	private readonly GameState _game;

	private readonly float _scoreMultiplier = 3;
	private float _speedMultiplier;
	private short _speed;
	private bool _isGameOver;
	private bool _isGameRunning;

	[ObservableProperty]
	private short _bestScore;
	[ObservableProperty]
	private short _score;
	[ObservableProperty]
	private string _speedStr;
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
			_speed = (short)(_speed * 0.99); // Уменьшаем скорость на 1%
			SpeedStr = $"{_speedMultiplier += 0.1f:F1}x"; // Увеличиваем коэффициент на 0.1 и обновляем строку
		}
	}

	private static void SetupGridWithEmptyCells(List<Image> gridImages)
	{
		for (byte row = 0; row < Rows; row++)
		{
			for (byte column = 0; column < Columns; column++)
			{
				var image = new Image()
				{
					Source = ImageService.SnakeImageSources["Empty"],
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
		await DrawAliveOrDeadSnake(SnakeModel.SnakeBody);
	}

	private void DrawGrid()
	{
		CellType cell;
		Image image;

		for (byte row = 0; row < Rows; row++)
		{
			for (byte column = 0; column < Columns; column++)
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
		SnakePart prevPart = snake.Last?.Previous?.Value;
		foreach (SnakePart currPart in snake)
		{
			DrawAliveOrDeadSnakePart(currPart, prevPart, isDead);
			prevPart = currPart;
			if (isDead)
			{
				await Task.Delay(50);
			}
		}
	}

	private void DrawAliveOrDeadSnakePart(SnakePart currPart, SnakePart prevPart, bool isDead)
	{
		Position position = currPart.Position;
		Direction direction = currPart.Direction;
		bool isTurn = currPart.IsTurn;

		SnakeTurnType turnType = SnakeModel.GetSnakeTurnType(direction, prevPart.Direction);
		SnakePartType partType = SnakeModel.GetSnakePartType(position);
		BitmapImage bitmapImage = SnakeModel.GetSnakeBitmapImage(partType, isTurn, isDead);

		int rotation = GetRotation(direction, turnType, isTurn);

		if (partType == SnakePartType.Tail)
			rotation = _directionToRotation[prevPart.Direction];

		SetSnakePartImage(position, bitmapImage, rotation);
	}

	private static int GetRotation(Direction direction, SnakeTurnType turnType, bool isTurn)
	{
		int rotation = _directionToRotation[direction];
		if (isTurn && turnType == SnakeTurnType.Clockwise)
			return rotation -= 270;
		return rotation;
	}

	private void SetSnakePartImage(Position position, BitmapImage bitmapImage, int rotation)
	{
		Image image = GridImages[position.Row * Columns + position.Column];
		image.Source = bitmapImage;
		image.RenderTransform = new RotateTransform(rotation);
	}

	private async Task ShowCountDown()
	{
		for (byte countDown = 3; countDown >= 1; countDown--)
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
		await DrawAliveOrDeadSnake(SnakeModel.SnakeBody, true);
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