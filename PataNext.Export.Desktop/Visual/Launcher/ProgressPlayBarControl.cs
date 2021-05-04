using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace PataNext.Export.Desktop.Visual
{
	public class ProgressPlayBarControl : Container
	{
		public readonly BindableBool  IsUpdating     = new();
		public readonly BindableFloat UpdateProgress = new();
		public          BindableBool  RequireRestart = new();

		public readonly Bindable<string> CurrentVersion = new("2021.05.03");

		private readonly ProgressBar progressBar;
		private readonly PlayButton  playButton;
		
		public ProgressPlayBarControl()
		{
			Children = new Drawable[]
			{
				progressBar = new()
				{
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
								
					RelativeSizeAxes = Axes.Both,
					Size             = new(0.7f, 0.45f),
				},
				playButton = new()
				{
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
								
					RelativeSizeAxes = Axes.Both,
					Size             = new(0.685f, 0.95f),
				},
				new SideDecoratorSprite
				{
					Origin = Anchor.CentreLeft,
					Anchor = Anchor.CentreLeft,
				},
				new SideDecoratorSprite
				{
					Scale = new(-1, 1),

					Origin = Anchor.CentreLeft, // because scale.x is -1
					Anchor = Anchor.CentreRight,
				}
			};

			IsUpdating.BindValueChanged(ev =>
			{
				playButton.ScaleTo(new Vector2(1, ev.NewValue ? 0 : 1), 100);
			}, true);
			
			RequireRestart.BindValueChanged(ev =>
			{
				playButton.TextToShow.Value = ev.NewValue ? "Restart" : "Play";
			}, true);
			
			UpdateProgress.BindValueChanged(ev =>
			{
				progressBar.Current.Value = ev.NewValue;
			}, true);
		}

		public class SideDecoratorSprite : Sprite
		{
			public SideDecoratorSprite()
			{
				FillMode = FillMode.Fit;

				RelativeSizeAxes = Axes.Both;
				Size             = new(1);
			}

			[BackgroundDependencyLoader]
			private void load(TextureStore textures)
			{
				Texture = textures.Get("progress_bar_side");
			}
		}
	
		public class ProgressBar : Container
		{
			public BindableFloat Current = new();

			private Box progressBox;
			public ProgressBar()
			{
				Children = new Drawable[]
				{
					new Box
					{
						RelativeSizeAxes = Axes.Both,
						Size             = new(1),
						Colour           = Colour4.Black
					},
					new Container
					{
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,

						RelativeSizeAxes = Axes.Both,
						Size             = new(0.99f, 0.825f),
						Children = new Drawable[]
						{
							progressBox = new()
							{
								Colour = Colour4.FromHex("42cb29"),

								Origin = Anchor.CentreLeft,
								Anchor = Anchor.CentreLeft,

								RelativeSizeAxes = Axes.Both,
								Size             = new(1)
							},
							new Box
							{
								Origin = Anchor.TopCentre,
								Anchor = Anchor.TopCentre,
								
								Colour = Colour4.Black.Opacity(0.4f),
								
								RelativeSizeAxes = Axes.Both,
								Size = new (1, 0.2f),
							},
							new Box
							{
								Origin = Anchor.BottomLeft,
								Anchor = Anchor.BottomLeft,
								
								Colour = Colour4.Black.Opacity(0.4f),
								
								RelativeSizeAxes = Axes.Both,
								Size             = new (0.018f, 0.8f),
							}
						}
					}
				};
				
				Current.BindValueChanged(ev =>
				{
					Debug.Assert(ev.NewValue is >= 0 and <= 1, "ev.NewValue is >= 0 and <= 1");

					progressBox.ScaleTo(new Vector2(ev.NewValue, 1), 120);
				}, true);
			}
		}

		public class PlayButton : Button
		{
			public Bindable<string> CurrentVersion = new();

			private SpriteText[] texts   = new SpriteText[3];
			private SpriteText[] effects = new SpriteText[2];
			private Container    effectContainer;

			private Box backgroundBox;

			public Bindable<LocalisableString> TextToShow = new("Play");
			
			public PlayButton()
			{
				Masking      = true;
				CornerRadius = 6;

				Children = new Drawable[]
				{
					backgroundBox = new()
					{
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,

						RelativeSizeAxes = Axes.Both,
						Size             = new(1),
						Colour           = Colour4.Black
					},

					// front
					texts[0] = new SpriteText
					{
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,

						Font    = new("mojipon", size: 40f, fixedWidth: true),
						Spacing = new(-13.5f, 0),
						Text    = "play",

						Colour = Colour4.FromHex("b81c31"),

						Position = new(0, 0)
					},

					effectContainer = new()
					{
						Origin = Anchor.Centre,
						Anchor = Anchor.Centre,
						
						Size = new (1),
						RelativeSizeAxes = Axes.Both,
						
						Children = new[]
						{
							// bg 1
							texts[1] = effects[0] = new()
							{
								Origin = Anchor.Centre,
								Anchor = Anchor.Centre,

								Font    = new("mojipon", size: 40f, fixedWidth: true),
								Spacing = new(-13.5f, 0),
								Text    = "play",

								Scale = new(1.4f),

								Colour = Colour4.FromHex("b81c31"),
								Alpha = 0.15f,

								Position = new(0, -0 * 1.6f)
							},
							// bg 1 (flip and bigger width)
							texts[2] = effects[1] = new()
							{
								Origin = Anchor.Centre,
								Anchor = Anchor.Centre,

								Font    = new("mojipon", size: 40f, fixedWidth: true),
								Spacing = new(-13.5f, 0),
								Text    = "play",

								Scale = new(-1.55f, 0.95f),

								Colour = Colour4.FromHex("b81c31"),
								Alpha  = 0.15f,

								Position = new(-12f, -0 * 1.6f)
							}
						}
					}
				};
				
				TextToShow.BindValueChanged(ev =>
				{
					foreach (var t in texts)
					{
						if (ev.NewValue.ToString() == "Restart")
						{
							t.Colour = Colour4.FromHex("42cb29");
						}
						else
						{
							t.Colour = Colour4.FromHex("b81c31");
						}
						
						t.Current.Value = ev.NewValue.ToString();
					}
				});
			}

			protected override bool OnHover(HoverEvent e)
			{
				if (mouseDown)
					return base.OnHover(e);
				
				foreach (var sprite in effects)
					sprite.FadeTo(0.2f, 100);

				this.ScaleTo(new Vector2(1.01f, 1f), 100);
				effectContainer.ScaleTo(1.1f, 100);
				
				return base.OnHover(e);
			}

			protected override void OnHoverLost(HoverLostEvent e)
			{
				if (mouseDown)
					return;
				
				foreach (var sprite in effects)
					sprite.FadeTo(0.15f, 100);

				this.ScaleTo(new Vector2(1f, 1f), 100);
				effectContainer.ScaleTo(1f, 100);
				
				base.OnHoverLost(e);
			}

			private bool mouseDown;
			protected override bool OnMouseDown(MouseDownEvent e)
			{
				mouseDown = true;
				
				foreach (var sprite in effects)
					sprite.FadeTo(0.22f, 100);

				this.ScaleTo(new Vector2(1.04f, 1f), 100);
				effectContainer.ScaleTo(new Vector2(1.225f, 1.2f), 100);
				
				return base.OnMouseDown(e);
			}

			protected override void OnMouseUp(MouseUpEvent e)
			{
				mouseDown = false;
				
				foreach (var sprite in effects)
					sprite.FadeTo(0.15f, 100);

				this.ScaleTo(new Vector2(1f, 1f), 100);
				effectContainer.ScaleTo(1f, 100);
				
				base.OnMouseUp(e);
			}
		}

		public Action PlayAction
		{
			get => playButton.Action;
			set => playButton.Action = value;
		}
	}
}