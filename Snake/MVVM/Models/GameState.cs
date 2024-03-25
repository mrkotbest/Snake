namespace Snake.MVVM.Models
{
    public class GameState
    {
		private readonly int _scoreMultiplier = 5;

		private float _speedMultiplier = 1.0f;

		public GameBoard GameBoard { get; private set; }
		public Snake Snake { get; private set; }
		public Food Food { get; private set; }

		public bool IsGameOver { get; private set; }
		public int Score { get; private set; }
		public int BestScore { get; private set; }
		public int Speed { get; private set; } = 100;
		public string SpeedDisplay { get => $"{_speedMultiplier:F2}x"; }

		public GameState(int rows, int columns)
        {
			GameBoard = new GameBoard(rows, columns);
			Snake = new Snake(GameBoard);
			Food = new Food(GameBoard);

			Snake.OnEatFood += HandleEatFood;
			Snake.OnGameOver += HandleGameOver;
		}

		public void Move()
		{
			Snake.Move();
		}

		public void ReInitialize()
		{
			_speedMultiplier = 1.0f;

			GameBoard.Initialize();
			Snake.Initialize();
			Food.Initialize();
			IsGameOver = false;
			Score = 0;
			Speed = 100;
		}

		private void HandleEatFood()
		{
			Score++;
			Food.AddFoodRandomly();

			if (Score % _scoreMultiplier == 0)
			{
				_speedMultiplier += 0.05f; // Увеличить множитель скорости
				Speed = (int)(100 / _speedMultiplier); // Увеличить скорость
			}
		}

		private void HandleGameOver()
		{
			IsGameOver = true;
			BestScore = Score > BestScore ? Score : BestScore;
		}
	}
}