using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace PataNext.Export.Desktop.Visual.Screens.Toolbar
{
    public class ToolbarRulesetTabButton : TabItem<GhMenuEntry>
    {
        private readonly RulesetButton ruleset;

        public ToolbarRulesetTabButton(GhMenuEntry value)
            : base(value)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Child = ruleset = new RulesetButton
            {
                Active = false,
            };

            ruleset.TooltipMain = value.Main;
            ruleset.TooltipSub = value.Sub;
            ruleset.SetIcon(value.Icon);

            /*var rInstance = value.CreateInstance();

            ruleset.TooltipMain = rInstance.Description;
            ruleset.TooltipSub = $"Play some {rInstance.Description}";
            ruleset.SetIcon(rInstance.CreateIcon());*/
        }

        protected override void OnActivated() => ruleset.Active = true;

        protected override void OnDeactivated() => ruleset.Active = false;

        private class RulesetButton : ToolbarButton
        {
            public bool Active
            {
                set
                {
                    if (value)
                    {
                        SelectedBackground.FadeIn(100);
                        
                        IconContainer.Colour = Color4.White;
                        IconContainer.EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Colour = new Color4(185, 38, 83, 100),
                            Radius = 15,
                            Roundness = 15,
                        };
                        
                       /* DrawableText.Text = TooltipMain;
                        this.ResizeWidthTo(WIDTH + DrawableText.DrawSize.X + 15, 250, Easing.In);*/
                    }
                    else
                    {
                        SelectedBackground.FadeOut(100);
                        
                        IconContainer.Colour = new Color4(185, 38, 83, 255);
                        IconContainer.EdgeEffect = new EdgeEffectParameters();
                        /*this.ResizeWidthTo(WIDTH, 250, Easing.Out);
                        DrawableText.Text = string.Empty;*/
                    }
                }
            }

            protected override bool OnClick(ClickEvent e)
            {
                Parent.Click();
                return base.OnClick(e);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                IconContainer.Scale *= 1.4f;
            }
        }
    }
}