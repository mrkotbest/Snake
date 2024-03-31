namespace Snake.MVVM.Models
{
    public class GameState
    {
		public GameBoard GameBoard { get; }
		public Snake Snake { get; }
		public Food Food { get; }

		public GameState(int rows, int columns)
        {
			GameBoard = new GameBoard(rows, columns);
			Snake = new Snake(GameBoard);
			Food = new Food(GameBoard);
		}

		public void Initialize()
		{
			GameBoard.Initialize();
			Snake.Initialize();
			Food.Initialize();
		}

		public CellType GetCurrentCell(int row, int column)
			=> GameBoard.Cells[row, column];

		public void Move()
			=> Snake.Move();

		public LinkedList<SnakePart> GetSnakeBody()
			=> Snake.SnakeBody;

		public void ChangeDirection(Direction direction)
			=> Snake.ChangeDirection(direction);

		public void AddFood()
			=> Food.AddFoodRandomly();
	}
}