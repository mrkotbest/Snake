namespace Snake.MVVM.Models
{
	public class SnakePart
	{
		public Direction Direction { get; set; }
		public Position Position { get; set; }
		public bool IsTurn { get; set; }
	}
}