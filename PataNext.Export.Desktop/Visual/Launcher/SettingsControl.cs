using System.Collections.Generic;
using GameHost.Injection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using PataNext.Export.Desktop.Visual.Overlays;
using Container = osu.Framework.Graphics.Containers.Container;

namespace PataNext.Export.Desktop.Visual
{
	public class SettingsControl : BasicScrollContainer
	{
		private Dictionary<string, FillFlowContainer<Drawable>> flowCategories = new();

		public const float WIDTH = 500;

		public readonly AlwaysUpdateFillFlowContainer<Drawable> MainFlow = new()
		{
			Size             = new(1, 800),
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes     = Axes.Y,

			Direction = FillDirection.Vertical,

			Anchor = Anchor.TopCentre,
			Origin = Anchor.TopCentre,

			Spacing = new(0, 30)
		};

		public SettingsControl()
		{
			Masking = true;
			Child   = MainFlow;
		}

		private void addCategory(string category, FillFlowContainer<Drawable> flow)
		{
			flow.Direction = FillDirection.Vertical;
			flow.Anchor    = Anchor.TopCentre;
			flow.Origin    = Anchor.TopCentre;

			flow.RelativeSizeAxes = Axes.X;
			flow.Size             = new Vector2(1, 0);
			
			flow.Spacing          = new(0, 5);

			flow.AutoSizeAxes = Axes.Y;

			flow.Add(new Container
			{
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre,

				Size = new(1, 20),
				RelativeSizeAxes = Axes.X,

				Children = new Drawable[]
				{
					new SpriteText
					{
						Size             = new(1f),
						RelativeSizeAxes = Axes.Both,

						Text   = category,
						Font   = new("ar_cena", 22),
						Colour = Colour4.FromRGBA(0x25110aff),

						Anchor = Anchor.CentreLeft,
						Origin = Anchor.CentreLeft,
					},
					new Box
					{
						Size             = new(1f, 2f),
						RelativeSizeAxes = Axes.X,

						Colour = Colour4.FromRGBA(0x25110aff),

						Anchor = Anchor.Centre,
						Origin = Anchor.Centre,

						Position = new(0, 11)
					}
				}
			});

			MainFlow.Add(flow);
		}

		public FillFlowContainer<Drawable> GetOrCreateCategoryFlow(string category)
		{
			if (flowCategories.TryGetValue(category, out var flow))
				return flow;

			addCategory(category, flowCategories[category] = flow = new AlwaysUpdateFillFlowContainer<Drawable>());
			return flow;
		}

		public void AddButton(string category, SettingsButtonBase button)
		{
			button.RelativeSizeAxes = Axes.X;
			button.Size             = new(1, button.Size.Y);
			button.Anchor           = Anchor.TopCentre;
			button.Origin           = Anchor.TopCentre;
			GetOrCreateCategoryFlow(category)
				.Add(button);
		}

		public void AddDrawable(string category, Drawable drawable)
		{
			GetOrCreateCategoryFlow(category)
				.Add(drawable);
		}
	}
}