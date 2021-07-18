using System.Collections.Generic;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using PataNext.Module.Simulation.Components.Army;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.SystemBase;

namespace PataNext.Module.Simulation.Defaults
{
	public class RegisterDefaultKits : GameAppSystem
	{
		private KitCollectionSystem kitCollectionSystem;
		private ResPathGen          resPathGen;

		public RegisterDefaultKits([NotNull] WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref kitCollectionSystem);
			DependencyResolver.Add(() => ref resPathGen);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			kitCollectionSystem.RegisterKit(handle =>
			{
				AddComponent(handle, new UnitSquadArmySelectorFromCenter(0));
				AddComponent(handle, new UnitTargetOffset {Idle = 0});
			}, new[] {resPathGen.GetDefaults().With(new[] {"kit", "hatadan"}, ResPath.EType.MasterServer)});
			
			kitCollectionSystem.RegisterKit(handle =>
			{
				AddComponent(handle, new UnitSquadArmySelectorFromCenter(2));
				AddComponent(handle, new UnitTargetOffset {Idle = 3});
			}, new[] {resPathGen.GetDefaults().With(new[] {"kit", "taterazay"}, ResPath.EType.MasterServer)});
			kitCollectionSystem.RegisterRole(handle =>
			{
				AddComponent(handle, new UnitStatistics()
				{
					Health = 150,
					Defense = 1,
					AttackSpeed = 2
				});
			}, new[] {resPathGen.GetDefaults().With(new[] {"role", "taterazay", "base"}, ResPath.EType.MasterServer)});
			
			kitCollectionSystem.RegisterKit(handle =>
			{
				AddComponent(handle, new UnitSquadArmySelectorFromCenter(1));
				AddComponent(handle, new UnitTargetOffset {Idle = 1});
			}, new[] {resPathGen.GetDefaults().With(new[] {"kit", "yarida"}, ResPath.EType.MasterServer)});
			kitCollectionSystem.RegisterRole(handle =>
			{
				AddComponent(handle, new UnitStatistics()
				{
					Health      = 120
				});
			}, new[] {resPathGen.GetDefaults().With(new[] {"role", "yarida", "base"}, ResPath.EType.MasterServer)});
			
			kitCollectionSystem.RegisterKit(handle =>
			{
				AddComponent(handle, new UnitSquadArmySelectorFromCenter(-1));
				AddComponent(handle, new UnitTargetOffset {Idle = -3});
			}, new[] {resPathGen.GetDefaults().With(new[] {"kit", "yumiyacha"}, ResPath.EType.MasterServer)});
			kitCollectionSystem.RegisterRole(handle =>
			{
				AddComponent(handle, new UnitStatistics()
				{
					Health = 100,
					AttackSeekRange = 8
				});
			}, new[] {resPathGen.GetDefaults().With(new[] {"role", "yumiyacha", "base"}, ResPath.EType.MasterServer)});
		}
	}
}