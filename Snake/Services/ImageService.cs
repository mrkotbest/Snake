using System.Windows.Media.Imaging;

namespace Snake.Services
{
	public static class ImageService
	{
		private static readonly string[] ImageNames = [ "Empty", "Food", "Head", "Body", "Tail", "Turn", "DeadHead", "DeadBody", "DeadTail", "DeadTurn" ];

		public static readonly Dictionary<string, BitmapImage> SnakeImageSources = [];

		static ImageService()
		{
			LoadSkin("Purple");
		}

		private static Dictionary<string, BitmapImage> LoadSkin(string skinType)
		{
			foreach (string name in ImageNames)
				SnakeImageSources[name] = LoadImage(skinType, $"{name}.png");

			return SnakeImageSources;
		}

		private static BitmapImage LoadImage(string foldername, string filename)
			=> TryLoadImage($"pack://application:,,,/Assets/Skins/{foldername}/{filename}");

		private static BitmapImage TryLoadImage(string path)
		{
			try
			{
				return GetBitmapImage(path);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to load image: {ex.Message} by current path: {path}");
				return new BitmapImage();
			}
		}

		private static BitmapImage GetBitmapImage(string path)
		{
			Uri uri = new(path, UriKind.Absolute);
			return new BitmapImage(uri);
		}
	}
}