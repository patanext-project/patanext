using osu.Framework.Input;
using osuTK.Input;

namespace PataNext.Export.Desktop.Visual
{
	public static class TranslateInput
	{
		public static string Get(JoystickButton key)
		{
			// todo
			return key.ToString();
		}

		public static string Get(Key key)
		{
			return key switch
			{
				Key.Left => "LeftArrow",
				Key.Right => "RightArrow",
				Key.Up => "UpArrow",
				Key.Down => "DownArrow",
				Key.Keypad0 => "Numpad0",
				Key.Keypad1 => "Numpad1",
				Key.Keypad2 => "Numpad2",
				Key.Keypad3 => "Numpad3",
				Key.Keypad4 => "Numpad4",
				Key.Keypad5 => "Numpad5",
				Key.Keypad6 => "Numpad6",
				Key.Keypad7 => "Numpad7",
				Key.Keypad8 => "Numpad8",
				Key.Keypad9 => "Numpad9",
				_ => key.ToString()
			};
		}
	}
}