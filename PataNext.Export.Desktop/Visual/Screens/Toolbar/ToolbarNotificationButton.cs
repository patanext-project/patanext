using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using PataNext.Export.Desktop.Visual.Overlays;

namespace PataNext.Export.Desktop.Visual.Screens.Toolbar
{
    public class ToolbarNotificationButton : ToolbarOverlayToggleButton
    {
        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public BindableInt NotificationCount = new BindableInt();

        private readonly CountCircle countDisplay;

        public ToolbarNotificationButton()
        {
            Icon        = FontAwesome.Solid.Bell;
            TooltipMain = "Notifications";

            Add(countDisplay = new CountCircle
            {
                Alpha                = 0,
                Height               = 16,
                RelativePositionAxes = Axes.Both,
                Origin               = Anchor.Centre,
                Position             = new Vector2(0.7f, 0.25f),
            });
        }

        [BackgroundDependencyLoader(true)]
        private void load(NotificationOverlay notificationOverlay)
        {
            StateContainer = notificationOverlay;

            if (notificationOverlay != null)
                NotificationCount.BindTo(notificationOverlay.UnreadCount);

            NotificationCount.ValueChanged += count =>
            {
                if (count.NewValue == 0)
                    countDisplay.FadeOut(200, Easing.OutQuint);
                else
                {
                    countDisplay.Count = count.NewValue;
                    countDisplay.FadeIn(200, Easing.OutQuint);
                }
            };
        }

        private class CountCircle : CompositeDrawable
        {
            private readonly SpriteText countText;
            private readonly Circle     circle;

            private int count;

            public int Count
            {
                get => count;
                set
                {
                    if (count == value)
                        return;

                    if (value > count)
                    {
                        circle.FlashColour(Color4.White, 600, Easing.OutQuint);
                        this.ScaleTo(1.1f).Then().ScaleTo(1, 600, Easing.OutElastic);
                    }

                    count          = value;
                    countText.Text = value.ToString("#,0");
                }
            }

            public CountCircle()
            {
                AutoSizeAxes = Axes.X;

                InternalChildren = new Drawable[]
                {
                    circle = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour           = Color4.Red
                    },
                    countText = new SpriteText
                    {
                        Anchor             = Anchor.Centre,
                        Origin             = Anchor.Centre,
                        Y                  = -1,
                        Font               = FontUsage.Default.With(size: 14),
                        Padding            = new MarginPadding(5),
                        Colour             = Color4.White,
                        UseFullGlyphHeight = true,
                    }
                };
            }
        }
    }
}