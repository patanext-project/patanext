using System;
using System.Runtime.InteropServices;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using PataNext.Module.Simulation.Components.GamePlay.Abilities;

namespace PataNext.Simulation.mixed.Components.GamePlay.Abilities
{
	public readonly struct SetupExecutableAbility : IComponentData
	{
		public delegate void Func(GameEntity self);

		public readonly IntPtr FunctionPtr;

		public Func Function => Marshal.GetDelegateForFunctionPointer<Func>(FunctionPtr);

		public SetupExecutableAbility(Func func)
		{
			FunctionPtr = Marshal.GetFunctionPointerForDelegate(func);
		}
	}

	public readonly struct ExecutableAbility : IComponentData
	{
		public delegate void Func(GameEntity owner, GameEntity self, ref AbilityState state);

		public readonly IntPtr FunctionPtr;

		public Func Function => Marshal.GetDelegateForFunctionPointer<Func>(FunctionPtr);

		public ExecutableAbility(Func func)
		{
			FunctionPtr = Marshal.GetFunctionPointerForDelegate(func);
		}
	}
}