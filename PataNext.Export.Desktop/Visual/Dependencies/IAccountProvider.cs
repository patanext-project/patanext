using osu.Framework.Bindables;

namespace PataNext.Export.Desktop.Visual.Dependencies
{
	public struct LauncherAccount
	{
		public bool IsConnected;
		
		public string Nickname;

		public bool   Error;
		public string Message;
	}

	public interface IAccountProvider
	{
		public Bindable<LauncherAccount> Current { get; }

		void ConnectTraditional(string login, string password);
		void Disconnect();
	}

	public interface IHasDiscordAccountSupport
	{
		void ConnectDiscord();
	}
}