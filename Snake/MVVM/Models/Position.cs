﻿namespace Snake.MVVM.Models
{
    public class Position(int row, int column)
    {
        public int Row { get; } = row;
        public int Column { get; } = column;

        public Position NextPosition(Direction direction)
        {
            Position nextPosition = new(Row + direction.ShiftRow, Column + direction.ShiftColumn);
            return nextPosition;
        }

        public Position PreviousPosition(Direction direction)
        {
			Position previousPosition = new(Row - direction.ShiftRow, Column - direction.ShiftColumn);
			return previousPosition;
		}

        public override bool Equals(object obj) => obj is Position position && Row == position.Row && Column == position.Column;

        public override int GetHashCode() => HashCode.Combine(Row, Column);

        public static bool operator ==(Position left, Position right) => EqualityComparer<Position>.Default.Equals(left, right);

        public static bool operator !=(Position left, Position right) => !(left == right);
    }
}