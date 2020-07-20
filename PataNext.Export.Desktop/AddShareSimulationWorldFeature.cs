using System;
using ENet;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Simulation.Application;
using GameHost.Simulation.Features.ShareWorldState;
using GameHost.Transports;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddShareSimulationWorldFeature : AppSystem
	{
		private ENetTransportDriver driver;
		
		public AddShareSimulationWorldFeature(WorldCollection collection) : base(collection)
		{
			driver = new ENetTransportDriver(8);
		}

		protected override void OnInit()
		{
			base.OnInit();

			var addr = new Address();
			addr.Port = 5945;
			
			driver.Bind(addr);
			driver.Listen();

			Console.WriteLine(addr.Port);

			World.Mgr.CreateEntity()
			     .Set<IFeature>(new ShareWorldStateFeature(driver));
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			
			driver.Update();
			
			while (driver.Accept().IsCreated)
			{}

			TransportEvent ev;
			while ((ev = driver.PopEvent()).Type != TransportEvent.EType.None)
			{
				Console.WriteLine(ev.Type);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			
			driver.Dispose();
		}
	}
}