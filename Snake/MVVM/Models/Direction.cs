namespace Snake.MVVM.Models
{
    public class Direction(int row, int column)
    {
        public readonly static Direction Left = new(0, -1);
        public readonly static Direction Up = new(-1, 0);
        public readonly static Direction Right = new(0, 1);
        public readonly static Direction Down = new(1, 0);

        public int ShiftRow { get; } = row;
        public int ShiftColumn { get; } = column;

        public Direction ReverseDirection() => new(-ShiftRow, -ShiftColumn);

        public override bool Equals(object obj) => obj is Direction direction && ShiftRow == direction.ShiftRow && ShiftColumn == direction.ShiftColumn;

        public override int GetHashCode() => HashCode.Combine(ShiftRow, ShiftColumn);

        public static bool operator ==(Direction left, Direction right) => EqualityComparer<Direction>.Default.Equals(left, right);

        public static bool operator !=(Direction left, Direction right) => !(left == right);
    }
}