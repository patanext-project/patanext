using System;
using ENet;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Simulation.Application;
using GameHost.Simulation.Features.ShareWorldState;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Transports;
using PataNext.Module.Simulation;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	public enum MessageType
	{
		Unknown        = 0,
		Rpc            = 10,
		InputData      = 50,
		SimulationData = 100
	}

	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddShareSimulationWorldFeature : AppSystem
	{
		private DataBufferWriter header;
		
		private HeaderTransportDriver driver;
		private ENetTransportDriver enetDriver;

		private GameWorld gameWorld;
		
		public AddShareSimulationWorldFeature(WorldCollection collection) : base(collection)
		{
			header = new DataBufferWriter(0);
			
			AddDisposable(enetDriver = new ENetTransportDriver(8));
			AddDisposable(driver = new HeaderTransportDriver(enetDriver));
			
			DependencyResolver.Add(() => ref gameWorld);
		}

		protected override void OnInit()
		{
			base.OnInit();

			var addr = new Address();
			addr.Port = 5945;
			
			enetDriver.Bind(addr);
			enetDriver.Listen();

			header.WriteInt((int) MessageType.SimulationData);
			driver.Header = header;

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
				switch (ev.Type)
				{
					case TransportEvent.EType.None:
						break;
					case TransportEvent.EType.RequestConnection:
						break;
					case TransportEvent.EType.Connect:
						break;
					case TransportEvent.EType.Disconnect:
						break;
					case TransportEvent.EType.Data:
						var reader = new DataBufferReader(ev.Data);
						var type   = (MessageType) reader.ReadValue<int>();
						switch (type)
						{
							case MessageType.Unknown:
								break;
							case MessageType.Rpc:
								break;
							case MessageType.SimulationData:
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			header.Dispose();
		}
	}
}