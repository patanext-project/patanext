using osu.Framework.Bindables;

namespace PataNext.Export.Desktop.Visual.Dependencies
{
	public interface IUpdatePatchDownload
	{
		Bindable<string> Version { get; }

		BindableBool  IsUpdating     { get; }
		BindableBool  RequireRestart { get; }
		BindableFloat Progress       { get; }

		void StartDownload();
		void UpdateAndRestart();
	}
}