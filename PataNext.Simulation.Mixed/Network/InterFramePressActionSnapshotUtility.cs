using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Utility.InterTick;

namespace PataNext.Module.Simulation.Network
{
	public static class InterFramePressActionSnapshotUtility
	{
		public static BitBuffer AddInterFrame(this BitBuffer bitBuffer, InterFramePressAction obj)
		{
			return bitBuffer.AddInt(obj.Pressed)
			                .AddInt(obj.Released);
		}

		public static BitBuffer AddInterFrameDelta(this BitBuffer bitBuffer, InterFramePressAction obj, InterFramePressAction baseline)
		{
			return bitBuffer.AddIntDelta(obj.Pressed, baseline.Pressed)
			                .AddIntDelta(obj.Released, baseline.Released);
		}

		public static InterFramePressAction ReadInterFrame(this BitBuffer bitBuffer)
		{
			InterFramePressAction obj;
			obj.Pressed  = bitBuffer.ReadInt();
			obj.Released = bitBuffer.ReadInt();
			return obj;
		}

		public static InterFramePressAction ReadInterFrameDelta(this BitBuffer bitBuffer, InterFramePressAction baseline)
		{
			InterFramePressAction obj;
			obj.Pressed  = bitBuffer.ReadIntDelta(baseline.Pressed);
			obj.Released = bitBuffer.ReadIntDelta(baseline.Released);
			return obj;
		}
	}
}