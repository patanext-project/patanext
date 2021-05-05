using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace PataNext.Export.Desktop.Visual
{
	public class Sidebar : Container
	{
		public readonly SidebarMenuTabControl Menu = new(Shearing, ShearingPos);
		
		public Sidebar()
		{
		}

		public const float Shearing = 0.35f;
		public const float ShearingPos = Shearing * 60;

		private bool flip;
		public bool Flip
		{
			get => flip;
			set
			{
				if (flip != value && shearedBox is { } box)
				{
					box.Position = new(-ShearingPos * (value ? -1 : 1), 0);
					box.Shear    = new(-Shearing * (value ? -1 : 1), 0);
				}
				
				flip = value;
			}
		}

		private Box shearedBox;

		[BackgroundDependencyLoader]
		private void load()
		{
			Menu.RelativeSizeAxes = Axes.Both;
			Menu.Size             = Vector2.One;

			AddRange(new Drawable[]
			{
				new Container()
				{
					Masking          = true,
					RelativeSizeAxes = Axes.Both,
					Size             = new(1, 1),
					
					Children = new Drawable[]
					{
						shearedBox = new()
						{
							EdgeSmoothness   = new(0.8f, 0),
							RelativeSizeAxes = Axes.Both,
							Colour           = Colour4.Black.Opacity(0.5f),
							Size             = new(1, 1),
							
							Position = new(-ShearingPos * (flip ? -1 : 1), 0),
							Shear    = new(-Shearing * (flip ? -1 : 1), 0)
						},
						Menu
					}
				}
			});
		}
	}
}