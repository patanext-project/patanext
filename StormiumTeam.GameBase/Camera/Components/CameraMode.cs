using BepuUtilities;
using GameHost.Injection;
using GameHost.Revolution.NetCode;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;

namespace StormiumTeam.GameBase.Camera.Components
{
	public enum CameraMode
	{
		/// <summary>
		///     The camera will not be ruled by this state and will revert to Default mode if there are
		///     no other states with '<see cref="Forced" />' mode.
		/// </summary>
		Default = 0,

		/// <summary>
		///     The camera will be forced to the rules of this state and override previous states.
		/// </summary>
		Forced = 1
	}

	public struct CameraState
	{
		public CameraMode Mode;

		public GameEntity         Target;
		public RigidTransform Offset;
	}

	public struct LocalCameraState : IComponentData
	{
		public CameraState Data;

		public CameraMode     Mode   => Data.Mode;
		public GameEntity     Target => Data.Target;
		public RigidTransform Offset => Data.Offset;
		
		public class Register : RegisterGameHostComponentData<LocalCameraState>
		{}
	}

	public struct ServerCameraState : IComponentData, IReadWriteComponentData<ServerCameraState, GhostSetup>
	{
		public CameraState Data;

		public CameraMode     Mode   => Data.Mode;
		public GameEntity     Target => Data.Target;
		public RigidTransform Offset => Data.Offset;

		public class Register : RegisterGameHostComponentData<ServerCameraState>
		{
		}

		public class Serializer : DeltaComponentSerializerBase<ServerCameraState, GhostSetup>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
			}
		}

		public void Serialize(in BitBuffer buffer, in ServerCameraState baseline, in GhostSetup setup)
		{
			buffer.AddUIntD4((byte) Data.Mode);
			buffer.AddGhostDelta(setup.ToGhost(Data.Target), default);
		}

		public void Deserialize(in BitBuffer buffer, in ServerCameraState baseline, in GhostSetup setup)
		{
			Data.Mode   = (CameraMode) buffer.ReadUIntD4();
			Data.Target = setup.FromGhost(buffer.ReadGhostDelta(default));
		}
	}
}