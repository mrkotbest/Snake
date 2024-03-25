namespace Snake.MVVM.Models
{
	public class GameBoard(int rows, int columns)
	{
		public int Rows { get; } = rows;
		public int Columns { get; } = columns;
		public CellValue[,] Cells { get; } = new CellValue[rows, columns];

		public void Initialize()
		{
			for (int row = 0; row < Rows; row++)
			{
				for (int column = 0; column < Columns; column++)
				{
					Cells[row, column] = CellValue.Empty;
				}
			}
		}

		public CellValue CellValueAtNewPosition(Position newPosition)
		{
			return IsCellOutside(newPosition) ? CellValue.Outside : Cells[newPosition.Row, newPosition.Column];
		}

		public bool IsCellOutside(Position position)
		{
			return position.Row < 0 || position.Row >= Rows || position.Column < 0 || position.Column >= Columns;
		}

		public IEnumerable<Position> EmptyCellPositions()
		{
			IEnumerable<int> rowRange = Enumerable.Range(0, Rows);
			IEnumerable<int> columnRange = Enumerable.Range(0, Columns);

			// Для каждой строки и каждого столбца проверяем, является ли ячейка пустой
			IEnumerable<Position> emptyCellPositions = from row in rowRange
													   from column in columnRange
													   where Cells[row, column] == CellValue.Empty
													   select new Position(row, column);

			return emptyCellPositions;
		}
	}
}