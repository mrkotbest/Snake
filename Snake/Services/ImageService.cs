using System.Windows.Media.Imaging;

namespace Snake.Services
{
	public class ImageService
	{
		public readonly static BitmapImage Empty = LoadImage("Purple", "Empty.png");
		public readonly static BitmapImage Food = LoadImage("Purple", "Food.png");
		public readonly static BitmapImage Head = LoadImage("Purple", "Head.png");
		public readonly static BitmapImage Body = LoadImage("Purple", "Body.png");
		public readonly static BitmapImage Tail = LoadImage("Purple", "Tail.png");
		public readonly static BitmapImage DeadHead = LoadImage("Purple", "DeadHead.png");
		public readonly static BitmapImage DeadBody = LoadImage("Purple", "DeadBody.png");
		public readonly static BitmapImage DeadTail = LoadImage("Purple", "DeadTail.png");
		public readonly static BitmapImage Turn = LoadImage("Purple", "Turn.png");
		public readonly static BitmapImage DeadTurn = LoadImage("Purple", "DeadTurn.png");

		private static BitmapImage LoadImage(string foldername, string filename)
			=> TryLoadImage($"pack://application:,,,/Assets/SnakeSkins/{foldername}/{filename}");

		private static BitmapImage TryLoadImage(string path)
		{
			try
			{
				return GetBitmapImage(path);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to load image: {ex.Message}");
				return null;
			}
		}

		private static BitmapImage GetBitmapImage(string path)
		{
			Uri uri = new(path, UriKind.Absolute);
			BitmapImage bitmapImage = new(uri);

			//if (bitmapImage.CanFreeze)
			//	bitmapImage.Freeze();

			return bitmapImage;
		}
	}
}