using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.Ecs.Passes;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;

namespace StormiumTeam.GameBase.SystemBase
{
	public abstract class GameLoopAppSystem : GameAppSystem
	{
		protected readonly bool IsAutomaticExecution;

		private List<ForEachExecutor> executors = new List<ForEachExecutor>();

		protected IScheduler LoopScheduler;

		public GameLoopAppSystem(WorldCollection collection) : base(collection)
		{
			throw new Exception("You need to set the second parameter 'isAutomatic' of this constructor.\nAnd your constructor should consist of only the 'WorldCollection' argument.");
		}

		public GameLoopAppSystem(WorldCollection collection, bool isAutomatic) : base(collection)
		{
			IsAutomaticExecution = isAutomatic;
			LoopScheduler        = new Scheduler();
		}

		public override bool CanUpdate()
		{
			LoopScheduler.Run();

			return base.CanUpdate();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (!IsAutomaticExecution)
				return;

			foreach (var exec in executors)
				exec.Run();
		}

		protected void RunExecutors()
		{
			if (IsAutomaticExecution)
				throw new Exception("IsAutomaticExecution is true. Create this system with 'isAutomatic' argument in constructor set to false.");

			foreach (var exec in executors)
				exec.Run();
		}

		protected void Add(ForEachExecutor executor)
		{
			executors.Add(executor);
		}

		protected void Add(Action<GameEntityHandle> action, EntityQuery query)
		{
			Add(new ForEachExecutorEntity {Action = action, Query = query});
		}

		protected void Add(Action<EntityQueryEnumerator> action, EntityQuery query)
		{
			Add(new ForEachExecutorEntityEnumerator {Action = action, Query = query});
		}


		protected void Add<T1>(ForEachExecutorEntity<T1>.Func action, EntityQuery query = null)
			where T1 : struct, IComponentData
		{
			LoopScheduler.ScheduleWithCondition(() =>
			{
				if (DependencyResolver.Dependencies.Any())
					return false;
				
				var final = QueryWith(query, new[] {typeof(T1)});
				Add(new ForEachExecutorEntity<T1> {Inner = this, Action = action, Query = final});

				return false;
			});
		}

		protected void Add<T1, T2>(ForEachExecutorEntity<T1, T2>.Func action, EntityQuery query = null)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
		{
			LoopScheduler.ScheduleWithCondition(() =>
			{
				if (DependencyResolver.Dependencies.Any())
					return false;

				var final = QueryWith(query, new[] { typeof(T1), typeof(T2) });
				Add(new ForEachExecutorEntity<T1, T2> { Inner = this, Action = action, Query = final });

				return true;
			});
		}

		protected void Add<T1, T2, T3>(ForEachExecutorEntity<T1, T2, T3>.Func action, EntityQuery query = null)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
			where T3 : struct, IComponentData
		{
			LoopScheduler.ScheduleWithCondition(() =>
			{
				if (DependencyResolver.Dependencies.Any())
					return false;

				var final = QueryWith(query, new[] { typeof(T1), typeof(T2), typeof(T3) });
				Add(new ForEachExecutorEntity<T1, T2, T3> { Inner = this, Action = action, Query = final });

				return true;
			});
		}
	}

	public abstract class ForEachExecutor
	{
		public abstract void Run();
	}
	
	public class ForEachExecutorEntity : ForEachExecutor
	{
		public Action<GameEntityHandle> Action;
		public EntityQuery              Query;
		
		public override void Run()
		{
			foreach (var entity in Query.GetEnumerator())
				Action(entity);
		}
	}
	
	public class ForEachExecutorEntityEnumerator : ForEachExecutor
	{
		public Action<EntityQueryEnumerator> Action;
		public EntityQuery        Query;
		
		public override void Run()
		{
			Action(Query.GetEnumerator());
		}
	}
	
	public class ForEachExecutorEntity<T1> : ForEachExecutor 
		where T1 : struct, IComponentData
	{
		public delegate bool Func(GameEntityHandle entity, ref T1 t1);
		
		public GameLoopAppSystem Inner;
		public Func              Action;
		public EntityQuery       Query;

		public override void Run()
		{
			var accessorT1 = Inner.GetAccessor<T1>();
			foreach (var entity in Query.GetEnumerator())
			{
				if (!Action(entity, ref accessorT1[entity]))
					break;
			}
		}
	}
	
	public class ForEachExecutorEntity<T1, T2> : ForEachExecutor 
		where T1 : struct, IComponentData
		where T2 : struct, IComponentData
	{
		public delegate bool Func(GameEntityHandle entity, ref T1 t1, ref T2 t2);
		
		public GameLoopAppSystem Inner;
		public Func              Action;
		public EntityQuery       Query;

		public override void Run()
		{
			var accessorT1 = Inner.GetAccessor<T1>();
			var accessorT2 = Inner.GetAccessor<T2>();
			foreach (var entity in Query.GetEnumerator())
			{
				if (!Action(entity, ref accessorT1[entity], ref accessorT2[entity]))
					break;
			}
		}
	}
	
	public class ForEachExecutorEntity<T1, T2, T3> : ForEachExecutor 
		where T1 : struct, IComponentData
		where T2 : struct, IComponentData
		where T3 : struct, IComponentData
	{
		public delegate bool Func(GameEntityHandle entity, ref T1 t1, ref T2 t2, ref T3 t3);
		
		public GameLoopAppSystem Inner;
		public Func              Action;
		public EntityQuery       Query;

		public override void Run()
		{
			var accessorT1 = Inner.GetAccessor<T1>();
			var accessorT2 = Inner.GetAccessor<T2>();
			var accessorT3 = Inner.GetAccessor<T3>();
			foreach (var entity in Query.GetEnumerator())
			{
				if (!Action(entity, ref accessorT1[entity], ref accessorT2[entity], ref accessorT3[entity]))
					break;
			}
		}
	}
}