using GameHost.Injection;
using GameHost.Native.Char;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Resources;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Module.Simulation.Network.Snapshots.Resources
{
	public struct GameGraphicResourceSnapshot : IReadWriteSnapshotData<GameGraphicResourceSnapshot>,
	                                              ISnapshotSyncWithComponent<GameGraphicResource>
	{
		public class Serializer : DeltaSnapshotSerializerBase<GameGraphicResourceSnapshot, GameGraphicResource>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public uint Tick { get; set; }

		public CharBuffer128 Identifier;

		public void Serialize(in BitBuffer buffer, in GameGraphicResourceSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (UnsafeUtility.SameData(this, baseline))
			{
				buffer.AddBool(false);
				return;
			}

			buffer.AddBool(true)
			      .AddString(Identifier.ToString());
		}

		public void Deserialize(in BitBuffer buffer, in GameGraphicResourceSnapshot baseline, in EmptySnapshotSetup setup)
		{
			if (!buffer.ReadBool())
			{
				this = baseline;
				return;
			}

			Identifier = CharBufferUtility.Create<CharBuffer128>(buffer.ReadString());
		}

		public void FromComponent(in GameGraphicResource component, in EmptySnapshotSetup setup)
		{
			Identifier = component.Value;
		}

		public void ToComponent(ref GameGraphicResource component, in EmptySnapshotSetup setup)
		{
			component = new GameGraphicResource(Identifier);
		}
	}
}