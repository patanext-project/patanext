using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;

namespace StormiumTeam.GameBase.SystemBase
{
	public partial class GameAppSystem
	{
		public void AddComponent<T1, T2>(GameEntity entity, T1 data1 = default, T2 data2 = default)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
		{
			GameWorld.AssureComponents(entity.Handle, stackalloc []
			{
				AsComponentType<T1>(),
				AsComponentType<T2>(),
			});
			GameWorld.AddComponent(entity.Handle, data1);
			GameWorld.AddComponent(entity.Handle, data2);
		}

		public void AddComponent<T1, T2, T3>(GameEntity entity, T1 data1 = default, T2 data2 = default, T3 data3 = default)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
			where T3 : struct, IComponentData
		{
			GameWorld.AssureComponents(entity.Handle, stackalloc []
			{
				AsComponentType<T1>(),
				AsComponentType<T2>(),
				AsComponentType<T3>(),
			});
			GameWorld.AddComponent(entity.Handle, data1);
			GameWorld.AddComponent(entity.Handle, data2);
			GameWorld.AddComponent(entity.Handle, data3);
		}

		public void AddComponent<T1, T2, T3, T4>(GameEntity entity, T1 data1 = default, T2 data2 = default, T3 data3 = default, T4 data4 = default)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
			where T3 : struct, IComponentData
			where T4 : struct, IComponentData
		{
			GameWorld.AssureComponents(entity.Handle, stackalloc []
			{
				AsComponentType<T1>(),
				AsComponentType<T2>(),
				AsComponentType<T3>(),
				AsComponentType<T4>(),
			});
			GameWorld.AddComponent(entity.Handle, data1);
			GameWorld.AddComponent(entity.Handle, data2);
			GameWorld.AddComponent(entity.Handle, data3);
			GameWorld.AddComponent(entity.Handle, data4);
		}

		public void AddComponent<T1, T2, T3, T4, T5>(GameEntity entity, T1 data1 = default, T2 data2 = default, T3 data3 = default, T4 data4 = default, T5 data5 = default)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
			where T3 : struct, IComponentData
			where T4 : struct, IComponentData
			where T5 : struct, IComponentData
		{
			GameWorld.AssureComponents(entity.Handle, stackalloc []
			{
				AsComponentType<T1>(),
				AsComponentType<T2>(),
				AsComponentType<T3>(),
				AsComponentType<T4>(),
				AsComponentType<T5>(),
			});
			GameWorld.AddComponent(entity.Handle, data1);
			GameWorld.AddComponent(entity.Handle, data2);
			GameWorld.AddComponent(entity.Handle, data3);
			GameWorld.AddComponent(entity.Handle, data4);
			GameWorld.AddComponent(entity.Handle, data5);
		}

		public void AddComponent<T1, T2, T3, T4, T5, T6>(GameEntity entity, T1 data1 = default, T2 data2 = default, T3 data3 = default, T4 data4 = default, T5 data5 = default, T6 data6 = default)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
			where T3 : struct, IComponentData
			where T4 : struct, IComponentData
			where T5 : struct, IComponentData
			where T6 : struct, IComponentData
		{
			GameWorld.AssureComponents(entity.Handle, stackalloc []
			{
				AsComponentType<T1>(),
				AsComponentType<T2>(),
				AsComponentType<T3>(),
				AsComponentType<T4>(),
				AsComponentType<T5>(),
				AsComponentType<T6>(),
			});
			GameWorld.AddComponent(entity.Handle, data1);
			GameWorld.AddComponent(entity.Handle, data2);
			GameWorld.AddComponent(entity.Handle, data3);
			GameWorld.AddComponent(entity.Handle, data4);
			GameWorld.AddComponent(entity.Handle, data5);
			GameWorld.AddComponent(entity.Handle, data6);
		}

		public void AddComponent<T1, T2, T3, T4, T5, T6, T7>(GameEntity entity, T1 data1 = default, T2 data2 = default, T3 data3 = default, T4 data4 = default, T5 data5 = default, T6 data6 = default, T7 data7 = default)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
			where T3 : struct, IComponentData
			where T4 : struct, IComponentData
			where T5 : struct, IComponentData
			where T6 : struct, IComponentData
			where T7 : struct, IComponentData
		{
			GameWorld.AssureComponents(entity.Handle, stackalloc []
			{
				AsComponentType<T1>(),
				AsComponentType<T2>(),
				AsComponentType<T3>(),
				AsComponentType<T4>(),
				AsComponentType<T5>(),
				AsComponentType<T6>(),
				AsComponentType<T7>(),
			});
			GameWorld.AddComponent(entity.Handle, data1);
			GameWorld.AddComponent(entity.Handle, data2);
			GameWorld.AddComponent(entity.Handle, data3);
			GameWorld.AddComponent(entity.Handle, data4);
			GameWorld.AddComponent(entity.Handle, data5);
			GameWorld.AddComponent(entity.Handle, data6);
			GameWorld.AddComponent(entity.Handle, data7);
		}
		
		public void AddComponent<T1, T2, T3, T4, T5, T6, T7, T8>(GameEntity entity, T1 data1 = default, T2 data2 = default, T3 data3 = default, T4 data4 = default, T5 data5 = default, T6 data6 = default, T7 data7 = default, T8 data8 = default)
			where T1 : struct, IComponentData
			where T2 : struct, IComponentData
			where T3 : struct, IComponentData
			where T4 : struct, IComponentData
			where T5 : struct, IComponentData
			where T6 : struct, IComponentData
			where T7 : struct, IComponentData
			where T8 : struct, IComponentData
		{
			GameWorld.AssureComponents(entity.Handle, stackalloc []
			{
				AsComponentType<T1>(),
				AsComponentType<T2>(),
				AsComponentType<T3>(),
				AsComponentType<T4>(),
				AsComponentType<T5>(),
				AsComponentType<T6>(),
				AsComponentType<T7>(),
				AsComponentType<T8>(),
			});
			GameWorld.AddComponent(entity.Handle, data1);
			GameWorld.AddComponent(entity.Handle, data2);
			GameWorld.AddComponent(entity.Handle, data3);
			GameWorld.AddComponent(entity.Handle, data4);
			GameWorld.AddComponent(entity.Handle, data5);
			GameWorld.AddComponent(entity.Handle, data6);
			GameWorld.AddComponent(entity.Handle, data7);
			GameWorld.AddComponent(entity.Handle, data8);
		}
	}
}