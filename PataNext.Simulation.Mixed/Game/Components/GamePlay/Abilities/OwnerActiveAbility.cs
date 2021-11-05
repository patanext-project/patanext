using GameHost.Native;
using GameHost.Native.Fixed;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.GamePlay.Abilities
{
	public struct OwnerActiveAbility : IComponentData
	{
		public int LastCommandActiveTime;
		public int LastActivationTime;

		public GameEntity PreviousActive;
		public GameEntity Active;
		public GameEntity Incoming;

		/// <summary>
		///     Current combo of the entity...
		/// </summary>
		public FixedBuffer32<GameResource<RhythmCommandResource>> CurrentCombo; //< 32 bytes should suffice, it would be 4 combo commands...

		public void AddCombo(GameResource<RhythmCommandResource> ent)
		{
			while (CurrentCombo.GetLength() >= CurrentCombo.GetCapacity())
				CurrentCombo.RemoveAt(0);
			CurrentCombo.Add(ent);
		}

		public bool RemoveCombo(GameResource<RhythmCommandResource> ent)
		{
			var index = CurrentCombo.IndexOf(ent);
			if (index < 0)
				return false;
			CurrentCombo.RemoveAt(index);
			return true;
		}

		public class Register : RegisterGameHostComponentData<OwnerActiveAbility>
		{
		}

		/*public class NetEmptySynchronizer : ComponentSnapshotSystemTag<OwnerActiveAbility>
		{
		}

		public void WriteTo(DataStreamWriter writer, ref OwnerActiveAbility baseline, GhostSetup setup, SerializeClientData jobData)
		{
			
		}

		public void ReadFrom(ref DataStreamReader.Context ctx, DataStreamReader reader, ref OwnerActiveAbility baseline, DeserializeClientData jobData)
		{
			
		}*/
	}
}