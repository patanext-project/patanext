using System;
using System.Threading;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Features.Systems;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using MagicOnion;
using MagicOnion.Client;

namespace StormiumTeam.GameBase.Network.MasterServer
{
	public enum RequestCallerStatus
	{
		/// <summary>
		/// This will destroy the caller entity
		/// </summary>
		DestroyCaller = 0,

		/// <summary>
		/// This will keep the caller entity alive.
		/// </summary>
		KeepCaller = 1
	}

	public struct InProcess<T> : IComponentData
	{
	}

	public struct UntrackedRequest : IComponentData
	{

	}

	public abstract class MasterServerRequestService<TService, TRequestComponent> : MasterServerRequestServiceMarkerDefaultEcs<TService, TRequestComponent>
		where TService : class, IService<TService>
	{
		public MasterServerRequestService(WorldCollection collection) : base(collection)
		{
		}
		
		protected override void OnFeatureAdded(Entity entity, MasterServerFeature obj)
		{
			Service = MagicOnionClient.Create<TService>(obj.Channel);
		}

		protected override void OnFeatureRemoved(Entity entity, MasterServerFeature obj)
		{
			Service = null!;
		}
	}
}