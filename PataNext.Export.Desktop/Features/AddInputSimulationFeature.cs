using System;
using System.Collections.Generic;
using DefaultEcs;
using ENet;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.Inputs.Features;
using GameHost.Inputs.Systems;
using GameHost.Simulation.Application;
using GameHost.Transports;
using GameHost.Worlds;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	[RestrictToApplication(typeof(SimulationApplication))]
	public class AddInputSimulationFeature : AppSystem
	{
		private HeaderTransportDriver driver;

		private DataBufferWriter header;

		private ReceiveInputDataSystem receiveInputDataSystem;
		private GlobalWorld            globalWorld;
		private IScheduler             selfScheduler;

		public AddInputSimulationFeature(WorldCollection collection) : base(collection)
		{
			header = new DataBufferWriter(0);
			DependencyResolver.Add(() => ref receiveInputDataSystem);
			DependencyResolver.Add(() => ref globalWorld);
			DependencyResolver.Add(() => ref selfScheduler);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);
			
			globalWorld.Scheduler.Schedule(mainLoopUpdate, SchedulingParameters.AsOnce);
		}

		protected override void OnInit()
		{
			base.OnInit();

			header.WriteInt((int) MessageType.InputData);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (driver == null)
				return;

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
					case TransportEvent.EType.Data:
						var reader = new DataBufferReader(ev.Data);
						var type   = (MessageType) reader.ReadValue<int>();
						switch (type)
						{
							case MessageType.InputData:
							{
								var inputDataReader = new DataBufferReader(reader, reader.CurrReadIndex, reader.Length);
								var subType         = (EMessageInputType) inputDataReader.ReadValue<int>();
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
			driver?.Dispose();
			header.Dispose();
		}
		
		private void mainLoopUpdate()
		{
			if (driver != null)
				return;

			foreach (var entity in globalWorld.World)
			{
				if (!entity.TryGet(out TransportAddress addr) || !entity.Has<ConnectionToInput>())
					continue;

				driver = new HeaderTransportDriver(addr.Connect()) {Header = header};
				break;
			}

			if (driver == null)
			{
				// continue scheduling until we have a valid driver
				globalWorld.Scheduler.Schedule(mainLoopUpdate, SchedulingParameters.AsOnce);
				return;
			}

			selfScheduler.Schedule(drv =>
			{
				var feature = new ClientInputFeature(drv, default);
				World.Mgr.CreateEntity()
				     .Set<IFeature>(feature);
			}, driver, default);
		}
	}
}