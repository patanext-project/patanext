using System.Numerics;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.GamePlay.Projectiles
{
	public readonly struct ProjectileEndedTag : IComponentData
	{
	}

	public readonly struct ProjectileExplodedEndReason : IComponentData
	{
	}

	public readonly struct ProjectileOutOfTimeEndReason : IComponentData
	{
	}
}