using Snake.Services;
using System.Windows.Media.Imaging;

namespace Snake.MVVM.Models
{
	public class Snake(GameBoard board)
	{
		private static readonly Dictionary<(Direction, Direction), SnakeTurnType> _turnTypeByDirection = new()
		{
			{(Direction.Up, Direction.Right), SnakeTurnType.Clockwise},
			{(Direction.Right, Direction.Down), SnakeTurnType.Clockwise},
			{(Direction.Down, Direction.Left), SnakeTurnType.Clockwise},
			{(Direction.Left, Direction.Up), SnakeTurnType.Clockwise}
		};

		private readonly GameBoard _gameBoard = board;

		public static LinkedList<SnakePart> SnakeBody { get; private set; } = new LinkedList<SnakePart>();
		public static LinkedList<Direction> ClipboardDirections { get; private set; } = new LinkedList<Direction>();

		public event Action OnEatFood;
		public event Action OnGameOver;

		public Direction CurrentDirection { get; private set; } = Direction.Right;

		public void Initialize()
		{
			SnakeBody.Clear();
			ClipboardDirections.Clear();
			CurrentDirection = Direction.Right;
			AddSnake();
		}

		public void Move()
		{
			UpdateDirection();

			Position newPosition = SnakeBody.First.Value.Position.NextPosition(CurrentDirection);
			CellType cellValue = _gameBoard.CellValueAtNewPosition(newPosition);

			switch (cellValue)
			{
				case CellType.Outside:
				case CellType.Snake:
					OnGameOver?.Invoke();
					break;
				case CellType.Empty:
					Grow(newPosition);
					RemoveTail();
					break;
				case CellType.Food:
					Grow(newPosition);
					OnEatFood?.Invoke();
					break;
			}
		}

		public void ChangeDirection(Direction direction)
		{
			if (ClipboardDirections.Count < 2 &&
				direction != LastClipboardDirection() &&
				direction != LastClipboardDirection().ReverseDirection())
			{
				ClipboardDirections.AddLast(direction);
			}
		}
		private Direction LastClipboardDirection()
			=> ClipboardDirections.Count == 0 ? CurrentDirection : ClipboardDirections.Last.Value;

		private void UpdateDirection()
		{
			if (ClipboardDirections.Count > 0)
			{
				CurrentDirection = ClipboardDirections.First.Value;
				ClipboardDirections.RemoveFirst();
			}
		}

		private void Grow(Position position)
		{
			UpdateTurnStatus();
			SnakePart snakePart = new() { Position = position, Direction = CurrentDirection, IsTurn = false };
			SnakeBody.AddFirst(snakePart);

			_gameBoard.Cells[position.Row, position.Column] = CellType.Snake;
		}
		private void UpdateTurnStatus()
		{
			if (SnakeBody.First?.Value.Direction != CurrentDirection && SnakeBody.First != null)
			{
				SnakeBody.First.Value.IsTurn = true;
			}
		}

		private void RemoveTail()
		{
			Position tailPosition = SnakeBody.Last.Value.Position;
			_gameBoard.Cells[tailPosition.Row, tailPosition.Column] = CellType.Empty;

			SnakeBody.RemoveLast();
		}

		private void AddSnake()
		{
			Position position;
			SnakePart snakePart;
			int middleRow = _gameBoard.Rows / 2;

			for (int column = 1; column <= 3; column++)
			{
				position = new Position(middleRow, column);
				snakePart = new SnakePart { Position = position, Direction = CurrentDirection, IsTurn = false };

				SnakeBody.AddFirst(snakePart);

				_gameBoard.Cells[position.Row, position.Column] = CellType.Snake;
			}
		}

		public static SnakeTurnType GetTurnType(Direction currentDirection, Direction previousDirection)
		{
			if (_turnTypeByDirection.TryGetValue((previousDirection, currentDirection), out SnakeTurnType turnType))
				return turnType;
			return SnakeTurnType.CounterClockwise;
		}

		public static SnakePartType GetSnakePartType(Position position)
		{
			if (position == SnakeBody.First.Value.Position)
				return SnakePartType.Head;
			else if (position == SnakeBody.Last.Value.Position)
				return SnakePartType.Tail;
			else
				return SnakePartType.Body;
		}

		public static BitmapImage GetSnakeImageSource(SnakePartType partType, bool isTurn, bool isDead = false)
		{
			return partType switch
			{
				SnakePartType.Head => isDead ? ImageService.SnakeImageSource["DeadHead"] : ImageService.SnakeImageSource["Head"],
				SnakePartType.Tail => isDead ? ImageService.SnakeImageSource["DeadTail"] : ImageService.SnakeImageSource["Tail"],
				SnakePartType.Body => isDead ? (isTurn ? ImageService.SnakeImageSource["DeadTurn"] : ImageService.SnakeImageSource["DeadBody"]) :
											   (isTurn ? ImageService.SnakeImageSource["Turn"] : ImageService.SnakeImageSource["Body"]),
				_ => null
			};
		}
	}
}