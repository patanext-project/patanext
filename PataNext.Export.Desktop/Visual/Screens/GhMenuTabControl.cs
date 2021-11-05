using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using PataNext.Export.Desktop.Visual.Screens.Toolbar;

namespace PataNext.Export.Desktop.Visual.Screens
{
	public class GhMenuTabControl : TabControl<GhMenuEntry>
	{
		protected override Dropdown<GhMenuEntry> CreateDropdown()
		{
			return null;
		}
		
		protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
		{
			RelativeSizeAxes = Axes.Y,
			AutoSizeAxes     = Axes.X,
			Direction        = FillDirection.Horizontal,
		};

		protected override TabItem<GhMenuEntry> CreateTabItem(GhMenuEntry value)
		{
			return new ToolbarRulesetTabButton(value);
		}
	}
}