using System.Windows.Media.Imaging;

namespace Snake.Services
{
	public class ImageService
	{
		public readonly static BitmapImage Empty = LoadImage("Empty.png");
		public readonly static BitmapImage Food = LoadImage("Food.png");
		public readonly static BitmapImage Head = LoadImage("Head.png");
		public readonly static BitmapImage Body = LoadImage("Body.png");
		public readonly static BitmapImage Tail = LoadImage("Tail.png");
		public readonly static BitmapImage DeadHead = LoadImage("DeadHead.png");
		public readonly static BitmapImage DeadBody = LoadImage("DeadBody.png");
		public readonly static BitmapImage DeadTail = LoadImage("DeadTail.png");

		private static BitmapImage LoadImage(string filename)
		{
			return TryLoadImage($"pack://application:,,,/Assets/{filename}");
		}

		private static BitmapImage TryLoadImage(string path)
		{
			try
			{
				Uri uri = new(path, UriKind.Absolute);
				BitmapImage bitmapImage = new(uri);

				if (bitmapImage.CanFreeze)
					bitmapImage.Freeze();

				return bitmapImage;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to load image: {ex.Message}");
				return null;
			}
		}
	}
}