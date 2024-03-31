namespace Snake.MVVM.Models
{
	public class GameBoard(int rows, int columns)
	{
		public int Rows { get; } = rows;
		public int Columns { get; } = columns;
		public CellType[,] Cells { get; } = new CellType[rows, columns];

		public void Initialize()
		{
			for (int row = 0; row < Rows; row++)
			{
				for (int column = 0; column < Columns; column++)
				{
					Cells[row, column] = CellType.Empty;
				}
			}
		}

		public IEnumerable<Position> EmptyCellPositions()
		{
			IEnumerable<int> rowRange = Enumerable.Range(0, Rows);
			IEnumerable<int> columnRange = Enumerable.Range(0, Columns);

			// Для каждой строки и каждого столбца проверяем, является ли ячейка пустой
			IEnumerable<Position> emptyCellPositions = from row in rowRange
													   from column in columnRange
													   where Cells[row, column] == CellType.Empty
													   select new Position(row, column);

			return emptyCellPositions;
		}

		public CellType CellValueAtNewPosition(Position newPosition)
			=> IsCellOutside(newPosition) ? CellType.Outside : Cells[newPosition.Row, newPosition.Column];

		private bool IsCellOutside(Position position)
			=> position.Row < 0 || position.Row >= Rows || position.Column < 0 || position.Column >= Columns;
	}
}