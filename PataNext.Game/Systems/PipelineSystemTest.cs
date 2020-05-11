using System;
using GameHost;
using GameHost.Applications;
using GameHost.Core.Applications;
using GameHost.Core.Ecs;

namespace PataponGameHost.Systems
{
	[RestrictToApplication(typeof(GameSimulationThreadingHost))]
	public class PipelineSystemTest : AppSystem
	{
		private InteractiveApplicationHost interApp;
		
		protected override void OnInit()
		{
			base.OnInit();
			interApp = new InteractiveApplicationHost();
			interApp.Listen();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			if (interApp.HasEvent())
			{
				var data = interApp.GetData();
				interApp.SendData(new byte[] { 1 });
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			interApp.Dispose();
		}

		public PipelineSystemTest(WorldCollection collection) : base(collection)
		{
		}
	}
}