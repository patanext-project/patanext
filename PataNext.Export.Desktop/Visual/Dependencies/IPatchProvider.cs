using osu.Framework.Bindables;

namespace PataNext.Export.Desktop.Visual.Dependencies
{
	public interface IPatchProvider
	{
		Bindable<string> Version { get; }

		BindableBool  IsUpdating     { get; }
		BindableBool  RequireRestart { get; }
		BindableFloat Progress       { get; }

		void StartDownload();
		void UpdateAndRestart();
	}
}