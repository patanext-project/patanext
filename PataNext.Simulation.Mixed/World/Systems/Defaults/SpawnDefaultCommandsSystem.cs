using System;
using System.Collections.Generic;
using GameHost.Applications;
using GameHost.Core.Ecs;
using GameHost.Injection;
using PataNext.Module.Simulation.Components.GamePlay.RhythmEngine.Structures;
using PataNext.Module.Simulation.Game.RhythmEngine;
using PataNext.Module.Simulation.Resources;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Systems
{
	public class SpawnDefaultCommandsSystem : GameAppSystem
	{
		public RhythmCommandResourceDb DataBase;

		public SpawnDefaultCommandsSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref DataBase);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			/*var result = new ContextBindingStrategy(Context, true).Resolve<IApplication>().AssignedEntity.Get<ApplicationName>();
			if (result.Value != "server")	// The problem is if it is a client, is that the commands will never get executed on the server,
											// since abilities are restricted to one command entity...
				return;*/
			
			AddComponent(DataBase.GetOrCreate(AsComponentType<MarchCommand>(), "march", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Pata),
				RhythmCommandAction.With(1, RhythmKeys.Pata),
				RhythmCommandAction.With(2, RhythmKeys.Pata),
				RhythmCommandAction.With(3, RhythmKeys.Pon),
			}).Handle, new MarchCommand());
			DataBase.GetOrCreate(AsComponentType<BackwardCommand>(), "backward", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Chaka),
				RhythmCommandAction.With(1, RhythmKeys.Pata),
				RhythmCommandAction.With(2, RhythmKeys.Chaka),
				RhythmCommandAction.With(3, RhythmKeys.Pata),
			});
			AddComponent(DataBase.GetOrCreate(AsComponentType<RetreatCommand>(), "retreat", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Pon),
				RhythmCommandAction.With(1, RhythmKeys.Pata),
				RhythmCommandAction.With(2, RhythmKeys.Pon),
				RhythmCommandAction.With(3, RhythmKeys.Pata),
			}).Handle, new RetreatCommand());
			AddComponent(DataBase.GetOrCreate(AsComponentType<JumpCommand>(), "jump", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Don),
				RhythmCommandAction.With(1, RhythmKeys.Don),
				RhythmCommandAction.With(2, RhythmKeys.Chaka),
				RhythmCommandAction.With(3, RhythmKeys.Chaka),
			}).Handle, new JumpCommand());
			DataBase.GetOrCreate(AsComponentType<AttackCommand>(), "attack", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Pon),
				RhythmCommandAction.With(1, RhythmKeys.Pon),
				RhythmCommandAction.With(2, RhythmKeys.Pata),
				RhythmCommandAction.With(3, RhythmKeys.Pon),
			});
			DataBase.GetOrCreate(AsComponentType<DefendCommand>(), "defend", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Chaka),
				RhythmCommandAction.With(1, RhythmKeys.Chaka),
				RhythmCommandAction.With(2, RhythmKeys.Pata),
				RhythmCommandAction.With(3, RhythmKeys.Pon),
			});
			DataBase.GetOrCreate(AsComponentType<ChargeCommand>(), "charge", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Pon),
				RhythmCommandAction.With(1, RhythmKeys.Pon),
				RhythmCommandAction.With(2, RhythmKeys.Chaka),
				RhythmCommandAction.With(3, RhythmKeys.Chaka),
			});
			DataBase.GetOrCreate(AsComponentType<PartyCommand>(), "party", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Pata),
				RhythmCommandAction.With(1, RhythmKeys.Pon),
				RhythmCommandAction.With(2, RhythmKeys.Don),
				RhythmCommandAction.With(3, RhythmKeys.Chaka),
			});
			DataBase.GetOrCreate(AsComponentType<SummonCommand>(), "summon", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Don),
				RhythmCommandAction.With(1, RhythmKeys.Don),
				RhythmCommandAction.WithOffset(1, 0.5f, RhythmKeys.Don),
				RhythmCommandAction.WithOffset(2, 0.5f, RhythmKeys.Don),
				RhythmCommandAction.WithOffset(3, 0, RhythmKeys.Don),
			});
			
			DataBase.GetOrCreate(AsComponentType<SkipCommand>(), "skip", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Chaka),
				RhythmCommandAction.With(1, RhythmKeys.Chaka),
				RhythmCommandAction.With(2, RhythmKeys.Pon),
				RhythmCommandAction.With(3, RhythmKeys.Pon),
			}, 3);

			// not yet
			AddComponent(DataBase.GetOrCreate(AsComponentType<QuickDefend>(), "quick_defend", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Chaka),
				RhythmCommandAction.WithSlider(1, 1, RhythmKeys.Pon)
			}, 4).Entity, new DefendCommand());
			AddComponent(DataBase.GetOrCreate(AsComponentType<QuickRetreat>(), "quick_retreat", new[]
			{
				RhythmCommandAction.WithSlider(0, 2, RhythmKeys.Pon),
				RhythmCommandAction.With(1, RhythmKeys.Pata)
			}, 4).Entity, new RetreatCommand());
		}
	}
}