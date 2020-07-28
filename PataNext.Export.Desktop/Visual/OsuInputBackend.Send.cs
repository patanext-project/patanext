using System;
using GameHost.Inputs.Features;
using GameHost.Inputs.Systems;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop.Visual
{
	public partial class OsuInputBackend
	{
		private void SendInputData(InputActionSystemGroup group)
		{
			var buffer = new DataBufferWriter(0);
			buffer.WriteInt((int) EMessageInputType.ReceiveInputs);

			var countMarker = buffer.WriteInt(0);
			var count       = 0;
			foreach (var system in group.Systems)
			{
				buffer.WriteStaticString(system.ActionPath);
				var lengthMarker = buffer.WriteInt(0);
				system.CallSerialize(ref buffer);
				buffer.WriteInt(buffer.Length - lengthMarker.GetOffset(sizeof(int)).Index, lengthMarker);

				count++;
			}

			buffer.WriteInt(count, countMarker);

			unsafe
			{
				driver.Broadcast(default, new Span<byte>((void*) buffer.GetSafePtr(), buffer.Length));
			}
		}
	}
}