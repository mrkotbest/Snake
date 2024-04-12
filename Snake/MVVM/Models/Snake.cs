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

		private void AddSnake()
		{
			Position position;
			int middleRow = _gameBoard.Rows / 2;

			for (int column = 1; column <= 3; column++)
			{
				position = new Position(middleRow, column);
				SnakeBody.AddFirst(new SnakePart(CurrentDirection, position, false));

				_gameBoard.Cells[position.Row, position.Column] = CellType.Snake;
			}
		}

		public void Move()
		{
			UpdateDirection();

			Position newPosition = SnakeBody.First.Value.Position.NextPosition(CurrentDirection);
			CellType cellType = _gameBoard.CellTypeAtNewPosition(newPosition);

			switch (cellType)
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

			SnakeBody.AddFirst(new SnakePart(CurrentDirection, position, false));

			_gameBoard.Cells[position.Row, position.Column] = CellType.Snake;
		}

		private void RemoveTail()
		{
			Position tailPosition = SnakeBody.Last.Value.Position;
			_gameBoard.Cells[tailPosition.Row, tailPosition.Column] = CellType.Empty;

			SnakeBody.RemoveLast();
		}

		private void UpdateTurnStatus()
		{
			if (SnakeBody.First.Value.Direction != CurrentDirection && SnakeBody.First != null)
			{
				SnakeBody.First.Value.IsTurn = true;
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

		public static SnakeTurnType GetSnakeTurnType(Direction currDirection, Direction prevDirection)
			=> _turnTypeByDirection.TryGetValue((prevDirection, currDirection), out SnakeTurnType turnType) ? turnType : SnakeTurnType.CounterClockwise;

		public static SnakePartType GetSnakePartType(Position position)
		{
			if (position == SnakeBody.First.Value.Position)
				return SnakePartType.Head;
			else if (position == SnakeBody.Last.Value.Position)
				return SnakePartType.Tail;
			else
				return SnakePartType.Body;
		}

		public static BitmapImage GetSnakeBitmapImage(SnakePartType partType, bool isTurn, bool isDead = false)
		{
			return partType switch
			{
				SnakePartType.Head => isDead ? ImageService.SnakeImageSources["DeadHead"] : ImageService.SnakeImageSources["Head"],
				SnakePartType.Tail => isDead ? ImageService.SnakeImageSources["DeadTail"] : ImageService.SnakeImageSources["Tail"],
				SnakePartType.Body => isDead ? (isTurn ? ImageService.SnakeImageSources["DeadTurn"] : ImageService.SnakeImageSources["DeadBody"]) :
											   (isTurn ? ImageService.SnakeImageSources["Turn"] : ImageService.SnakeImageSources["Body"]),
				_ => null
			};
		}
	}
}