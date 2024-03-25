namespace Snake.MVVM.Models
{
	public class Food(GameBoard board)
	{
		private readonly GameBoard _gameBoard = board;
		private readonly Random _random = new();

		public void Initialize()
		{
			AddFoodRandomly();
		}

		public void AddFoodRandomly()
		{
			List<Position> emptyPositions = _gameBoard.EmptyCellPositions().ToList();
			if (emptyPositions.Count != 0)
			{
				Position randomPosition = emptyPositions[_random.Next(emptyPositions.Count)];
				_gameBoard.Cells[randomPosition.Row, randomPosition.Column] = CellValue.Food;
			}
		}
	}
}