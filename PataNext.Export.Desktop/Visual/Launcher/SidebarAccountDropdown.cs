using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using PataNext.Export.Desktop.Visual.Dependencies;
using SharpInputSystem;

namespace PataNext.Export.Desktop.Visual
{
	public class SidebarAccountDropdown : Container
	{
		private readonly Container  background;
		private readonly SpriteIcon dropdownIcon;
		private readonly SpriteText nameText;

		private readonly Dropdown dropdown;
		
		public SidebarAccountDropdown()
		{
			Children = new Drawable[]
			{
				background = new Container
				{
					Masking = true,
					
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					
					Size             = new (1),
					RelativeSizeAxes = Axes.Both,
					
					Alpha = 0,
					
					Child = 					new Box()
					{
						Shear    = new(-Sidebar.Shearing, 0),
						Position = new(-Sidebar.ShearingPos, 0),
						Colour   = Colour4.Black,

						Size             = new(1),
						RelativeSizeAxes = Axes.Both
					}
				},
				
				dropdown = new Dropdown()
				{
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					
					Size             = new (1.5f, 1),
					RelativeSizeAxes = Axes.Both,
				},
				
				new Container {
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					
					Size   = new (12, 12),
					Margin = new() {Left = 13},
					
					Child = dropdownIcon = new SpriteIcon
					{
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre,
						Icon   = FontAwesome.Solid.AngleDown,

						Size             = new(1),
						RelativeSizeAxes = Axes.Both
					}
				},
				new Container
				{
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					
					Margin = new() {Left = 50},
					Size = new(1, 0.5f),
					RelativeSizeAxes = Axes.Both,
					
					Child = nameText = new SpriteText
					{
						Anchor = Anchor.CentreLeft,
						Origin = Anchor.CentreLeft,
						Text   = "Not Connected",

						Font = new("ar_cena", 29f),
						
						Size             = new(1, 1),
						RelativeSizeAxes = Axes.Both,
						
						Position = new(0, -4)
					}
				}
			};
			
			dropdown.Active.BindValueChanged(ev =>
			{
				dropdownIcon.RotateTo(ev.NewValue ? 180 : 0, 300, Easing.InOutQuint);
			});
		}

		protected override bool OnHover(HoverEvent e)
		{
			background.FadeTo(0.4f, 75);
			
			return base.OnHover(e);
		}

		protected override void OnHoverLost(HoverLostEvent e)
		{
			background.FadeTo(0, 75);
			
			base.OnHoverLost(e);
		}

		private IAccountProvider accountProvider;
		
		[BackgroundDependencyLoader]
		private void load(IAccountProvider accountProvider)
		{
			this.accountProvider = accountProvider;
			
			accountProvider.Current.BindValueChanged(onAccountChange, true);
		}

		private void onAccountChange(ValueChangedEvent<LauncherAccount> ev)
		{
			if (ev.NewValue.IsConnected)
			{
				nameText.Text = ev.NewValue.Nickname;
			}
			else
			{
				nameText.Text = "Not Connected";
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (accountProvider != null)
				accountProvider.Current.ValueChanged -= onAccountChange;
			
			base.Dispose(isDisposing);
		}

		class Dropdown : Container
		{
			public BindableBool Active { get; } = new();

			private Container container;

			private FillFlowContainer connectContainer;
			private FillFlowContainer disconnectContainer;

			private TextBox loginField, passwordField;
			
			public Dropdown()
			{
				Children = new Drawable[]
				{
					container = new()
					{
						Size = new Vector2(1, 240),
						RelativeSizeAxes = Axes.X,
						
						Anchor = Anchor.BottomLeft,
						Origin = Anchor.TopLeft,
						
						Children = new Drawable[]
						{
							new Box()
							{
								Size             = new (1),
								RelativeSizeAxes = Axes.Both,
								
								Colour = Colour4.Black.Opacity(0.9f)
							},
							
							connectContainer = new()
							{
								Size = new (1),
								RelativeSizeAxes = Axes.Both,
								
								Direction = FillDirection.Vertical,
								
								Padding = new(10),
								
								Children = new Drawable[]
								{
									new SpriteText
									{
										Text = "Login",
										Font = new("ar_cena", 25)
									},
									loginField = new BasicTextBox
									{
										Size = new Vector2(1, 30),
										RelativeSizeAxes = Axes.X
									},
									new Sprite {Height = 10},
									new SpriteText
									{
										Text = "Password",
										Font = new("ar_cena", 25)
									},
									passwordField = new BasicPasswordTextBox()
									{
										Size             = new Vector2(1, 30),
										RelativeSizeAxes = Axes.X
									},
									new Sprite {Height = 20},
									new BasicButton()
									{
										Size             = new Vector2(1, 40),
										RelativeSizeAxes = Axes.X,
										
										Text = "Connect",
										
										Action = connect
									},
									new Sprite {Height = 10},
									new BasicButton()
									{
										Size             = new Vector2(1, 30),
										RelativeSizeAxes = Axes.X,
										
										BackgroundColour = Colour4.FromHex("7289DA"),
										
										Text = "Connect With Discord",
										
										Action = connectDiscord
									}
								}
							},
							
							disconnectContainer = new()
							{
								Size             = new (1),
								RelativeSizeAxes = Axes.Both,
								
								Direction = FillDirection.Vertical,
								
								Padding = new(10),
								
								Children = new Drawable[]
								{
									new BasicButton()
									{
										Size             = new Vector2(1, 40),
										RelativeSizeAxes = Axes.X,
										
										Text = "Disconnect",
										
										BackgroundColour = Colour4.DarkRed,
										
										Action = disconnect
									}
								}
							}
						}
					}
				};
				
				Active.BindValueChanged(ev =>
				{
					container.FadeTo(ev.NewValue ? 1 : 0, 50);
				}, true);
			}

			private void connect()
			{
				accountProvider.ConnectTraditional(loginField.Text, passwordField.Text);
				Active.Value = false;
			}

			private void connectDiscord()
			{
				if (accountProvider is IHasDiscordAccountSupport discordAccountSupport)
				{
					discordAccountSupport.ConnectDiscord();
					Active.Value = false;
				}
			}

			private void disconnect()
			{
				accountProvider.Disconnect();
			}

			protected override bool OnClick(ClickEvent e)
			{
				Active.Value = !Active.Value;
				
				return base.OnClick(e);
			}

			[Resolved]
			private IAccountProvider accountProvider { get; set; }

			protected override void LoadComplete()
			{
				base.LoadComplete();

				accountProvider.Current.BindValueChanged(ev =>
				{
					if (ev.NewValue.IsConnected)
					{
						connectContainer.Alpha    = 0;
						disconnectContainer.Alpha = 1;
						
						container.Height = 60;
					}
					else
					{
						connectContainer.Alpha    = 1;
						disconnectContainer.Alpha = 0;

						container.Height = 240;
					}
				}, true);
			}
		}
	}
}