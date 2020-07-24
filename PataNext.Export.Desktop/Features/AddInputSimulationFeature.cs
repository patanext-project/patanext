using System;
using ENet;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Inputs.Features;
using GameHost.Inputs.Systems;
using GameHost.Simulation.Application;
using GameHost.Transports;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddInputSimulationFeature : AppSystem
	{
		private ENetTransportDriver enetDriver;
		private HeaderTransportDriver driver;

		private DataBufferWriter header;

		private ReceiveInputDataSystem receiveInputDataSystem;
		
		public AddInputSimulationFeature(WorldCollection collection) : base(collection)
		{
			header = new DataBufferWriter(0);
			
			AddDisposable(enetDriver = new ENetTransportDriver(1));
			AddDisposable(driver = new HeaderTransportDriver(enetDriver)
			{
				Header = header
			});
			
			DependencyResolver.Add(() => ref receiveInputDataSystem);
		}

		protected override void OnInit()
		{
			base.OnInit();

			header.WriteInt((int) MessageType.InputData);

			var addr = new Address();
			addr.SetIP("127.0.0.1");
			addr.Port = 5962;
			enetDriver.Connect(addr);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			driver.Update();

			while (driver.Accept().IsCreated)
			{
			}

			var isFirstFrame = false;
			
			receiveInputDataSystem.BeginFrame();
			
			TransportEvent ev;
			while ((ev = driver.PopEvent()).Type != TransportEvent.EType.None)
			{
				switch (ev.Type)
				{
					case TransportEvent.EType.Connect:
						World.Mgr.CreateEntity()
						     .Set<IFeature>(new ClientInputFeature(driver, default));
						break;
					case TransportEvent.EType.Data:
						var reader = new DataBufferReader(ev.Data);
						var type   = (MessageType) reader.ReadValue<int>();
						switch (type)
						{
							case MessageType.InputData:
							{
								var inputDataReader = new DataBufferReader(reader, reader.CurrReadIndex, reader.Length);
								var subType = (EMessageInputType) inputDataReader.ReadValue<int>();
								switch (subType)
								{
									case EMessageInputType.None:
										break;
									case EMessageInputType.Register:
										throw new NotImplementedException($"GameHost shouldn't receive {nameof(EMessageInputType.Register)} event");
									case EMessageInputType.ReceiveRegister:
									{
										break;
									}
									case EMessageInputType.ReceiveInputs:
									{
										receiveInputDataSystem?.ReceiveData(ref inputDataReader);
										break;
									}
									default:
										throw new ArgumentOutOfRangeException();
								}

								break;
							}
						}

						break;
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