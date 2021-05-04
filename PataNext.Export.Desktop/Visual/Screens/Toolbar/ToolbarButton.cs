using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace PataNext.Export.Desktop.Visual.Screens.Toolbar
{
    public class ToolbarButton : ClickableContainer
    {
        public const float WIDTH = Toolbar.HEIGHT * 1.4f + 10;

        public void SetIcon(Drawable icon)
        {
            IconContainer.Icon = icon;
            IconContainer.Show();
        }

        public void SetIcon(IconUsage icon) =>
            SetIcon(new SpriteIcon
            {
                Size = new Vector2(20),
                Icon = icon
            });

        public IconUsage Icon
        {
            set => SetIcon(value);
        }

        public string Text
        {
            get => DrawableText.Text.ToString();
            set => DrawableText.Text = value;
        }

        public string TooltipMain
        {
            get => tooltip1.Text.ToString();
            set => tooltip1.Text = value;
        }

        public string TooltipSub
        {
            get => tooltip2.Text.ToString();
            set => tooltip2.Text = value;
        }

        protected virtual Anchor TooltipAnchor => Anchor.TopLeft;

        protected        ConstrainedIconContainer IconContainer;
        protected        SpriteText               DrawableText;
        protected        Box                      HoverBackground;
        protected        Box                      SelectedBackground;
        private readonly FillFlowContainer        tooltipContainer;
        private readonly SpriteText               tooltip1;
        private readonly SpriteText               tooltip2;
        protected        FillFlowContainer        Flow;

        public ToolbarButton()
        {
            Width            = WIDTH;
            RelativeSizeAxes = Axes.Y;

            Padding = new MarginPadding {Horizontal = 1};
            Children = new Drawable[]
            {
                HoverBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour           = Colour4.DarkGray.Darken(0.5f).Opacity(100),
                    Blending         = BlendingParameters.Additive,
                    Alpha            = 0,
                },
                Flow = new FillFlowContainer
                {
                    Direction        = FillDirection.Horizontal,
                    Spacing          = new Vector2(5),
                    Anchor           = Anchor.Centre,
                    Origin           = Anchor.Centre,
                    Padding          = new MarginPadding {Left = Toolbar.HEIGHT / 2, Right = Toolbar.HEIGHT / 2},
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes     = Axes.X,
                    Children = new Drawable[]
                    {
                        IconContainer = new ConstrainedIconContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size   = new Vector2(20),
                            Alpha  = 0,
                        },
                        DrawableText = new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Padding = new MarginPadding {Left = 10}
                        }
                    },
                },
                SelectedBackground = new Box
                {
                    Anchor           = Anchor.TopCentre,
                    Origin           = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Size             = new Vector2(1, 3),
                    Colour           = Colour4.IndianRed
                },
                tooltipContainer = new FillFlowContainer
                {
                    Direction        = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.Both, //stops us being considered in parent's autosize
                    Anchor           = TooltipAnchor.HasFlag(Anchor.x0) ? Anchor.BottomLeft : Anchor.BottomRight,
                    Origin           = TooltipAnchor,
                    Position         = new Vector2(TooltipAnchor.HasFlag(Anchor.x0) ? 5 : -5, 5),
                    Alpha            = 0,
                    Children = new[]
                    {
                        tooltip1 = new SpriteText
                        {
                            Anchor = TooltipAnchor,
                            Origin = TooltipAnchor,
                            Shadow = true,
                            Font   = FontUsage.Default.With(family: "OpenSans-Bold", size: 28),
                            ShadowOffset = new Vector2(0.02f),
                            ShadowColour = Colour4.Black.MultiplyAlpha(1f)
                        },
                        tooltip2 = new SpriteText
                        {
                            Anchor = TooltipAnchor,
                            Origin = TooltipAnchor,
                            Shadow = true,
                            Font = FontUsage.Default.With(size: 20)
                        }
                    }
                }
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;
        

        protected override bool OnClick(ClickEvent e)
        {
            HoverBackground.FlashColour(Color4.White.Opacity(100), 250, Easing.OutQuint);
            tooltipContainer.FadeOut(100);
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            HoverBackground.FadeIn(200);
            tooltipContainer.FadeIn(100);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            HoverBackground.FadeOut(200);
            tooltipContainer.FadeOut(100);
        }
    }

    public class OpaqueBackground : Container
    {
        public OpaqueBackground()
        {
            RelativeSizeAxes  = Axes.Both;
            Masking           = true;
            MaskingSmoothness = 0;
            EdgeEffect = new EdgeEffectParameters
            {
                Type   = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour           = Colour4.Gray.Darken(0.25f)
                }
            };
        }
    }
}