using osu.Framework.Bindables;

namespace PataNext.Export.Desktop.Visual.Dependencies
{
	public interface ICurrentVersion
	{
		Bindable<string> Current { get; }
	}
}