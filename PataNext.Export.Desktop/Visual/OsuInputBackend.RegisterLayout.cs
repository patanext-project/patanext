using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Inputs.Components;
using GameHost.Inputs.Features;
using GameHost.Inputs.Layouts;
using GameHost.Inputs.Systems;
using GameHost.Native.Char;
using GameHost.Simulation.TabEcs;
using GameHost.Transports;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using PataNext.Export.Desktop.Visual.Systems;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop.Visual
{
	public partial class OsuInputBackend : Container
	{
		private void OnRegisterLayoutActions(TransportConnection connection, ref DataBufferReader reader)
		{
			if (!worldCollection.TryGet(out InputBackendSystem inputBackendSystem))
				throw new InvalidOperationException($"No {nameof(OsuInputBackend)} found");

			if (!worldCollection.TryGet(out OsuInputBackendSystem osuBackend))
				throw new InvalidOperationException($"No {nameof(OsuInputBackend)} found");

			if (!worldCollection.TryGet(out OsuInputBackendRegisterLayoutSystem registerLayoutSystem))
				throw new InvalidOperationException($"No {nameof(OsuInputBackend)} found");

			osuBackend.ClearCurrentActions();

			var length = reader.ReadValue<int>();
			for (var ac = 0; ac < length; ac++)
			{
				var actionId   = reader.ReadValue<int>();
				var actionType = reader.ReadString();
				var skipAction = reader.ReadValue<int>();

				var actionEntity = osuBackend.RegisterAction(connection, actionType, new InputEntityId(actionId));
				if (actionEntity == default)
				{
					Logger.Log($"No type defined for action '{actionType}'", LoggingTarget.Database, level: LogLevel.Error);

					reader.CurrReadIndex += skipAction;
					continue;
				}

				var layoutCount = reader.ReadValue<int>();

				var layouts = inputBackendSystem.GetLayoutsOf(actionEntity);
				for (var lyt = 0; lyt < layoutCount; lyt++)
				{
					var layoutId   = reader.ReadString();
					var layoutType = reader.ReadString();

					var startLayout = reader.CurrReadIndex;
					var skipLayout = reader.ReadValue<int>();

					var layout = registerLayoutSystem.TryCreateLayout(layoutType, layoutId);
					if (layout == null)
					{
						Logger.Log($"No type defined for layout '{layoutType}'", LoggingTarget.Database, level: LogLevel.Error);

						reader.CurrReadIndex += skipLayout;
						continue;
					}

					layout.Deserialize(ref reader);
					if (reader.CurrReadIndex != startLayout + skipLayout)
						throw new InvalidOperationException($"Error when deserializing {layoutType} ({reader.CurrReadIndex} != {startLayout + skipLayout}={startLayout} + {skipLayout})"); 
					
					layouts.Add(layout);
				}
			}
		}
	}
}