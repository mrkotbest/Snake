namespace Snake.MVVM.Models
{
	public class SnakePart(Direction direction, Position position, bool isTurn)
	{
		public Direction Direction { get; private set; } = direction;
		public Position Position { get; private set; } = position;
		public bool IsTurn { get; set; } = isTurn;
	}
}