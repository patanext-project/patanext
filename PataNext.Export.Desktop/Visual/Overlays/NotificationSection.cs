﻿using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
 using osu.Framework.Input.Events;
 using osuTK;

namespace PataNext.Export.Desktop.Visual.Overlays
{
    public class NotificationSection : AlwaysUpdateFillFlowContainer<Drawable>
    {
        private SpriteText countDrawable;

        private FlowContainer<Notification> notifications;

        public int DisplayedCount => notifications.Count(n => !n.WasClosed);
        public int UnreadCount    => notifications.Count(n => !n.WasClosed && !n.Read);

        public void Add(Notification notification, float position)
        {
            notifications.Add(notification);
            notifications.SetLayoutPosition(notification, position);
        }

        public IEnumerable<Type> AcceptTypes;

        private readonly string clearButtonText;

        private readonly string titleText;

        public NotificationSection(string title, string clearButtonText)
        {
            this.clearButtonText = clearButtonText;
            titleText            = title;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes     = Axes.Y;
            Direction        = FillDirection.Vertical;

            Padding = new MarginPadding
            {
                Top    = 10,
                Bottom = 5,
                Right  = 20,
                Left   = 20,
            };

            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes     = Axes.Y,
                    Children = new Drawable[]
                    {
                        new ClearAllButton
                        {
                            Text   = clearButtonText,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Action = clearAll
                        },
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding
                            {
                                Bottom = 5
                            },
                            Spacing      = new Vector2(5, 0),
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Text = titleText.ToUpperInvariant(),
                                    Font = FontUsage.Default.With(size: 24)
                                },
                                countDrawable = new SpriteText
                                {
                                    Text   = "3",
                                    Colour = ColourInfo.SingleColour(Colour4.IndianRed)
                                },
                            }
                        },
                    },
                },
                notifications = new AlwaysUpdateFillFlowContainer<Notification>
                {
                    AutoSizeAxes     = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    LayoutDuration   = 150,
                    LayoutEasing     = Easing.OutQuart,
                    Spacing          = new Vector2(3),
                }
            });
        }

        private void clearAll()
        {
            notifications.Children.ForEach(c => c.Close());
        }

        protected override void Update()
        {
            base.Update();

            countDrawable.Text = notifications.Children.Count(c => c.Alpha > 0.99f).ToString();
        }

        private class ClearAllButton : ClickableContainer
        {
            private readonly SpriteText text;

            public ClearAllButton()
            {
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    text = new SpriteText {Alpha = 0.5f}
                };
            }

            public string Text
            {
                get => text.Text;
                set => text.Text = value.ToUpperInvariant();
            }

            protected override bool OnHover(HoverEvent e)
            {
                text.FadeTo(1, 75);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                text.FadeTo(0.5f, 75);
                base.OnHoverLost(e);
            }
        }

        public void MarkAllRead()
        {
            notifications?.Children.ForEach(n => n.Read = true);
        }
    }

    public class AlwaysUpdateFillFlowContainer<T> : FillFlowContainer<T>
        where T : Drawable
    {
        // this is required to ensure correct layout and scheduling on children.
        // the layout portion of this is being tracked as a framework issue (https://github.com/ppy/osu-framework/issues/1297).
        protected override bool RequiresChildrenUpdate => true;
    }
}