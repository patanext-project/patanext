using BepuUtilities;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

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

	public struct ServerCameraState : IComponentData
	{
		public CameraState Data;

		public CameraMode     Mode   => Data.Mode;
		public GameEntity     Target => Data.Target;
		public RigidTransform Offset => Data.Offset;
		
		public class Register : RegisterGameHostComponentData<ServerCameraState>
		{}
	}
}