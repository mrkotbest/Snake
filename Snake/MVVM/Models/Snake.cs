namespace Snake.MVVM.Models
{
	public class Snake
	{
		private readonly GameBoard _gameBoard;

		public event Action OnEatFood;
		public event Action OnGameOver;

		public LinkedList<Position> SnakeBody { get; private set; } = new LinkedList<Position>();
		public LinkedList<Direction> ClipboardDirections { get; private set; } = new LinkedList<Direction>();
		public LinkedList<Direction> DirectionsHistory { get; private set; } = new LinkedList<Direction>();

		public Direction CurrentDirection { get; private set; }

		public Snake(GameBoard board)
		{
			_gameBoard = board;

			SnakeBody = new LinkedList<Position>();
			ClipboardDirections = new LinkedList<Direction>();

			CurrentDirection = Direction.Right;

			AddSnake();
		}

		private void AddSnake()
		{
			int middleRow = _gameBoard.Rows / 2;
			for (int column = 1; column <= 3; column++)
			{
				Position position = new(middleRow, column);
				SnakeBody.AddFirst(position);
				_gameBoard.Cells[position.Row, position.Column] = CellValue.Snake;

				DirectionsHistory.AddFirst(CurrentDirection);
			}
		}

		public void Initialize()
		{
			SnakeBody.Clear();
			ClipboardDirections.Clear();
			DirectionsHistory.Clear();
			CurrentDirection = Direction.Right;
			AddSnake();
		}


		public void Move()
		{
			UpdateDirection();

			Position newPosition = HeadPosition().NextPosition(CurrentDirection);
			CellValue cellValue = _gameBoard.CellValueAtNewPosition(newPosition);

			switch (cellValue)
			{
				case CellValue.Outside:
				case CellValue.Snake:
					OnGameOver?.Invoke();
					break;
				case CellValue.Empty:
					MoveSnake(newPosition);
					break;
				case CellValue.Food:
					EatFood(newPosition);
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

		private void MoveSnake(Position newPosition)
		{
			Grow(newPosition);
			RemoveTail();
		}

		private void EatFood(Position newPosition)
		{
			Grow(newPosition);
		}

		private void Grow(Position position)
		{
			SnakeBody.AddFirst(position);
			_gameBoard.Cells[position.Row, position.Column] = CellValue.Snake;

			DirectionsHistory.AddFirst(CurrentDirection);
		}

		private void RemoveTail()
		{
			Position position = TailPosition();
			_gameBoard.Cells[position.Row, position.Column] = CellValue.Empty;
			SnakeBody.RemoveLast();

			DirectionsHistory.RemoveLast();
		}

		public void ChangeDirection(Direction direction)
		{
			if (ClipboardDirections.Count < 2 &&
				direction != LastDirection() && 
				direction != LastDirection().ReverseDirection())
			{
				ClipboardDirections.AddLast(direction);
			}
		}

		private Direction LastDirection()
		{
			return ClipboardDirections.Count == 0 ? CurrentDirection : ClipboardDirections.Last.Value;
		}

		public Direction LastDirectionFromDirectionHistory()
		{
			return DirectionsHistory.Last?.Previous?.Value ?? throw new InvalidOperationException("Last Snake direction not found.");
		}

		public Position HeadPosition()
		{
			return SnakeBody.First?.Value ?? throw new InvalidOperationException("First Snake position is empty.");
		}

		public Position TailPosition()
		{
			return SnakeBody.Last?.Value ?? throw new InvalidOperationException("Last Snake position is empty."); ;
		}
	}
}