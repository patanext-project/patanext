using System;
using System.Collections.Generic;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.RPC;
using GameHost.Injection;
using GameHost.Simulation.TabEcs;
using GameHost.Utility;
using Microsoft.Extensions.Logging;
using NetFabric.Hyperlinq;
using PataNext.Game.Rpc.SerializationUtility;
using PataNext.Module.Simulation.Components.Hideout;
using StormiumTeam.GameBase.SystemBase;
using ZLogger;

namespace PataNext.Simulation.Client.Rpc
{
	public struct SetEquipmentUnitRpc : IGameHostRpcPacket
	{
		public GameEntity UnitEntity;

		// Key is Attachment
		// Value is Item entity
		public Dictionary<string, SerializableEntity> Targets;

		public class Process : RpcPacketSystem<SetEquipmentUnitRpc>
		{
			private ILogger logger;

			public Process(WorldCollection collection) : base(collection)
			{
				DependencyResolver.Add(() => ref logger);
			}

			public override string MethodName => "PataNext.SetEquipmentUnit";

			protected override void OnNotification(SetEquipmentUnitRpc notification)
			{
				var app = GetClientAppUtility.Get(this);
				app.Schedule(args =>
				{
					var (request, application, log) = args;
					var gameWorld = new ContextBindingStrategy(application.Data.Context, true).Resolve<GameWorld>();

					var unitEntity = new SafeEntityFocus(gameWorld, request.UnitEntity);
					if (!unitEntity.Exists())
					{
						log.ZLogError($"Unit {request.UnitEntity} doesn't exist");
						return;
					}
					
					app.Data.World
					     .CreateEntity()
					     .Set(new UpdateUnitEquipmentRequest(unitEntity, request.Targets.ToDictionary(pair => pair.Key, pair => (Entity)pair.Value)));
				}, (notification, app, logger), default);
			}
		}
	}
}