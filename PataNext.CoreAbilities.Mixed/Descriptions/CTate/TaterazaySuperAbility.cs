using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.BaseSystems;
using PataNext.Simulation.Mixed.Components.GamePlay.RhythmEngine.DefaultCommands;
using StormiumTeam.GameBase;

namespace PataNext.CoreAbilities.Mixed.CTate
{
	public struct TaterazaySuperAbility : IComponentData
	{
		public float Speed;
		public float JumpPower;

		public struct State : IComponentData
		{
			public int LastUpdateActivation;
		}
	}
	
	public class TaterazaySuperAbilityProvider : BaseRuntimeRhythmAbilityProvider<TaterazaySuperAbility>
	{
		public TaterazaySuperAbilityProvider(WorldCollection collection) : base(collection)
		{
			DefaultConfiguration = new TaterazaySuperAbility()
			{
				Speed = 1,
				JumpPower = 8
			};
		}

		protected override string FilePath       => "tate";
		public override    string MasterServerId => resPath.Create(new[] { "ability", "taterazay", "super" }, ResPath.EType.MasterServer);
		public override ComponentType GetChainingCommand()
		{
			return AsComponentType<MarchCommand>();
		}

		public override void GetComponents(PooledList<ComponentType> entityComponents)
		{
			base.GetComponents(entityComponents);
			
			entityComponents.Add(AsComponentType<TaterazaySuperAbility.State>());
		}

		public override void SetEntityData(GameEntityHandle entity, CreateAbility data)
		{
			base.SetEntityData(entity, data);
		}
	}
}