using System.Diagnostics;
using GameHost.Core.Client;
using GameHost.Game;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;

namespace PataNext.Export.Desktop.Visual.Screens.Section
{
	public class GameSectionScreen : Screen
    {
        [Resolved]
        private GameBootstrap gameBootstrap { get; set; }

        protected override void LoadComplete()
		{
			base.LoadComplete();

            RelativeSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One,
                
                Padding = new MarginPadding {Horizontal = 10},
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Size             = new Vector2(1, 100),
                        Origin           = Anchor.BottomCentre,
                        Anchor           = Anchor.BottomCentre,
                        
                        Margin = new MarginPadding {Bottom = 10},

                        CornerRadius = 13f,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                
                                RelativeSizeAxes = Axes.X,
                                Size             = new Vector2(1f, 120),
                                Colour           = Colour4.Black.MultiplyAlpha(0.25f)
                            },
                            new KButton
                            {
                                Origin = Anchor.CentreRight,
                                Anchor = Anchor.CentreRight,
                                
                                Padding = new MarginPadding {Horizontal = 10},
                                
                                Size = new Vector2(300, 38),
                                Position = new Vector2(0, -22),
                                Text             = "Play",
                                Action = () =>
                                {
                                    var world = gameBootstrap.Global.World;
                                    if (world.Get<GameClient>().Length > 0)
                                    {
                                        world.Get<VisualHWND>()[0].RequireSwap = true;
                                        return;
                                    }
                                    
                                    foreach (var entity in world)
                                    {
                                        if (!entity.Has<ClientBootstrap>())
                                            continue;

                                        var launch = world.CreateEntity();
                                        launch.Set(new LaunchClient(entity));
                                        break;
                                    }
                                }
                            },
                            new UpdateButton
                            {
                                Origin = Anchor.CentreRight,
                                Anchor = Anchor.CentreRight,
                                
                                Padding = new MarginPadding {Horizontal = 10},
                                
                                Size     = new Vector2(300, 38),
                                Position = new Vector2(0, 22),
                                Text     = "Check for Updates",
                                Action = () => { }
                            }
                        }
                    }
                }
            };
        }

        public class KButton : Button
        {
            public string Text
            {
                get => SpriteText?.Text.ToString();
                set
                {
                    if (SpriteText != null)
                        SpriteText.Text = value;
                }
            }

            private Color4? backgroundColour;

            public Color4 BackgroundColour
            {
                set
                {
                    backgroundColour = value;
                    Background.FadeColour(value);
                }
            }

            protected override Container<Drawable> Content { get; }

            protected Box        Hover;
            protected Box        Background;
            protected SpriteText SpriteText;

            public KButton()
            {
                Height = 50;

                AddInternal(Content = new Container
                {
                    Anchor           = Anchor.Centre,
                    Origin           = Anchor.Centre,
                    Masking          = true,
                    CornerRadius     = 10,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Background = new Box
                        {
                            Anchor           = Anchor.Centre,
                            Origin           = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                        },
                        Hover = new Box
                        {
                            Alpha            = 0,
                            Anchor           = Anchor.Centre,
                            Origin           = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour           = Color4.White.Opacity(.1f),
                            Blending         = BlendingParameters.Additive,
                            Depth            = float.MinValue
                        },
                        SpriteText = CreateText(),
                    }
                });

                Enabled.BindValueChanged(enabledChanged, true);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                if (backgroundColour == null)
                    BackgroundColour = Colour4.IndianRed.Opacity(0.75f);

                Enabled.ValueChanged += enabledChanged;
                Enabled.TriggerChange();
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (Enabled.Value)
                {
                    Debug.Assert(backgroundColour != null);
                    Background.FlashColour(backgroundColour.Value, 200);
                }

                return base.OnClick(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (Enabled.Value)
                    Hover.FadeIn(200, Easing.OutQuint);

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                Hover.FadeOut(300);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                Content.ScaleTo(0.9f, 2000, Easing.OutQuint);
                return base.OnMouseDown(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                Content.ScaleTo(1, 500, Easing.OutElastic);
                base.OnMouseUp(e);
            }

            protected virtual SpriteText CreateText() =>
                new SpriteText
                {
                    Depth  = -1,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Font = new FontUsage("OpenSans-Bold", size: 30)
                };

            private void enabledChanged(ValueChangedEvent<bool> e)
            {
                this.FadeColour(e.NewValue ? Color4.White : Color4.Gray, 200, Easing.OutQuint);
            }
        }
    }
}