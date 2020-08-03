using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Game;
using GameHost.Inputs.Features;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.Transports;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK.Input;
using PataNext.Export.Desktop.Visual.Systems;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop.Visual
{
	public partial class OsuInputBackend : Container
	{
		private WorldCollection       worldCollection;
		private HeaderTransportDriver driver;

		[BackgroundDependencyLoader]
		private void load(GameBootstrap gameBootstrap)
		{
			worldCollection = new WorldCollection(gameBootstrap.Global.Context, new World());
			worldCollection.Mgr.CreateEntity()
			               .Set(new InputCurrentLayout());
			
			var systems = new List<Type>
			{
				typeof(InputActionSystemGroup),
				typeof(ReceiveInputDataSystem)
			};
			AppSystemResolver.ResolveFor<IApplication>(systems, type => { return typeof(InputActionSystemBase).IsAssignableFrom(type); });
			AppSystemResolver.ResolveFor<IOsuFrameworkApplication>(systems);
			foreach (var sysType in systems)
			{
				worldCollection.GetOrCreate(sysType);
			}

			var threadDriver = new ThreadTransportDriver(1);
			threadDriver.Listen();

			driver = new HeaderTransportDriver(threadDriver);
			var writer = new DataBufferWriter(0);
			writer.WriteInt((int) MessageType.InputData);
			driver.Header = writer;

			var displayedAddr = gameBootstrap.Global.Collection.Mgr.CreateEntity();
			displayedAddr.Set(driver.TransportAddress);
			displayedAddr.Set<ConnectionToInput>();

			var inputBackend = worldCollection.GetOrCreate(c => new InputBackendSystem(c));
			foreach (Key key in Enum.GetValues(typeof(Key)))
			{
				inputBackend.GetOrCreateInputControl($"keyboard/{TranslateInput.Get(key)}");
			}
			foreach (JoystickButton key in Enum.GetValues(typeof(JoystickButton)))
			{
				inputBackend.GetOrCreateInputControl($"joystick/{TranslateInput.Get(key)}");
			}
		}

		protected override void Update()
		{
			base.Update();

			worldCollection.LoopPasses();
			if (worldCollection.TryGet(out InputActionSystemGroup @group))
				@group.BackendUpdateInputs();

			if (worldCollection.TryGet(out InputBackendSystem backendSystem))
			{
				foreach (var inputControl in backendSystem.inputDataMap)
				{
					inputControl.Value.wasPressedThisFrame  = false;
					inputControl.Value.wasReleasedThisFrame = false;
				}
			}

			if (driver == null)
				return;

			// Send data
			if (group != null)
				SendInputData(group);

			driver.Update();
			while (driver.Accept().IsCreated)
			{
			}

			TransportEvent ev;
			while ((ev = driver.PopEvent()).Type != TransportEvent.EType.None)
			{
				if (ev.Type != TransportEvent.EType.Data)
					continue;

				var reader = new DataBufferReader(ev.Data);
				var type   = (MessageType) reader.ReadValue<int>();
				if (type != MessageType.InputData)
					continue;

				var inputReader  = new DataBufferReader(reader, reader.CurrReadIndex, reader.Length);
				var inputMsgType = (EMessageInputType) inputReader.ReadValue<int>();
				switch (inputMsgType)
				{
					case EMessageInputType.None:
						break;
					case EMessageInputType.Register:
						OnRegisterLayoutActions(ev.Connection, ref inputReader);
						break;
					case EMessageInputType.ReceiveRegister:
						break;
					case EMessageInputType.ReceiveInputs:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		protected override bool OnKeyDown(KeyDownEvent e)
		{
			if (!worldCollection.TryGet(out InputBackendSystem backendSystem))
				return base.OnKeyDown(e);

			var inputControl = backendSystem.GetOrCreateInputControl(string.Format("keyboard/{0}", TranslateInput.Get(e.Key)));
			inputControl.wasPressedThisFrame = !e.Repeat;
			inputControl.isPressed           = true;
			inputControl.axisValue           = 1;

			return base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyUpEvent e)
		{
			if (!worldCollection.TryGet(out InputBackendSystem backendSystem))
				return;

			var inputControl = backendSystem.GetOrCreateInputControl(string.Format("keyboard/{0}", TranslateInput.Get(e.Key)));
			inputControl.wasReleasedThisFrame = true;
			inputControl.isPressed            = false;
			inputControl.axisValue            = 0;
		}

		protected override bool OnJoystickPress(JoystickPressEvent e)
		{
			if (!worldCollection.TryGet(out InputBackendSystem backendSystem))
				return base.OnJoystickPress(e);

			var inputControl = backendSystem.GetOrCreateInputControl(string.Format("joystick/{0}", e.Button));
			inputControl.wasPressedThisFrame = true;
			inputControl.isPressed           = true;
			inputControl.axisValue           = 1;

			return base.OnJoystickPress(e);
		}

		protected override void OnJoystickRelease(JoystickReleaseEvent e)
		{
			if (!worldCollection.TryGet(out InputBackendSystem backendSystem))
				return;

			var inputControl = backendSystem.GetOrCreateInputControl(string.Format("joystick/{0}", e.Button));
			inputControl.wasReleasedThisFrame = true;
			inputControl.isPressed            = false;
			inputControl.axisValue            = 0;
		}

		protected override bool OnJoystickAxisMove(JoystickAxisMoveEvent e)
		{
			if (!worldCollection.TryGet(out InputBackendSystem backendSystem))
				return base.OnJoystickAxisMove(e);

			var inputControl = backendSystem.GetOrCreateInputControl(string.Format("joystick/{0}", e.Axis));
			inputControl.isPressed = e.LastValue > 0.1;
			inputControl.axisValue = e.LastValue;

			return base.OnJoystickAxisMove(e);
		}
	}
}