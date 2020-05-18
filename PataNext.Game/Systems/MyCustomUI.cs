using System;
using GameHost.Applications;
using GameHost.Core;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.UI.Noesis;
using Noesis;
using OpenToolkit.Windowing.Common;
using EventArgs = Noesis.EventArgs;

namespace PataNext.Systems
{
	[RestrictToApplication(typeof(GameRenderThreadingHost))]
	[UpdateAfter(typeof(NoesisInitializationSystem))] // todo: we should not do that, but instead OnInit() should run after all dependencies has been resolved
	public class MyCustomUI : AppSystem
	{
		[DependencyStrategy]
		public INativeWindow Window { get; set; }

		public MyCustomUI(WorldCollection w) : base(w)
		{
			
		}

		protected override void OnInit()
		{
			base.OnInit();

			var gui = new NoesisOpenTkRenderer(Window);

			gui.ParseXaml(@"
<UserControl x:class=""PataNext.Game.Interface""
             xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
			xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
			xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
			xmlns:local=""clr-namespace:PataNext.Game""
			xmlns:noesis=""clr-namespace:NoesisGUIExtensions;assembly=Noesis.GUI.Extensions""
			d:DesignHeight=""1080"" d:DesignWidth=""1920"">
                <Grid>
                    <Viewbox>
                        <StackPanel Margin=""50"">
                            <Button Content=""pog button"" Margin=""0,30,0,0""/>
                            <Rectangle Height=""5"" Margin=""-10,20,-10,0"">
                                <Rectangle.Fill>
                                    <RadialGradientBrush>
                                        <GradientStop Offset=""0"" Color=""#40000000""/>
                                        <GradientStop Offset=""1"" Color=""#00000000""/>
                                    </RadialGradientBrush>
                                </Rectangle.Fill>
                            </Rectangle>
                        </StackPanel>
                    </Viewbox>
                </Grid>
</UserControl>");

			/*World.Mgr.CreateEntity()
			     .Set(gui);*/
		}
	}
}

namespace PataNext.Game
{
	public class Interface : UserControl
	{
		public Interface()
		{
			Initialized += OnInitialized;
		}

		private void OnInitialized(object sender, EventArgs args)
		{
			Console.WriteLine("hello!");
		}
	}
}