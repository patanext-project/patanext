using System.Collections.Generic;
using System.Text.Json;
using GameHost.Applications;
using GameHost.Core.Ecs;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Utility.Misc;

namespace StormiumTeam.GameBase.Bootstrap
{
	public readonly struct TargetBootstrap
	{
		public readonly string NameId;

		public TargetBootstrap(string nameId) => NameId = nameId;
	}

	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public abstract class BootstrapEntry : AppSystem
	{
		public virtual string Id          => TypeExt.GetFriendlyName(GetType());
		public virtual string DisplayName => GetType().Name;

		private BootstrapManager bootstrapManager;

		protected BootstrapEntry(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref bootstrapManager);
		}

		protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
		{
			base.OnDependenciesResolved(dependencies);

			bootstrapManager.AddEntry(Id, DisplayName, OnExecute);
		}

		protected abstract void OnExecute(string jsonArgs);
	}

	[RestrictToApplication(typeof(ExecutiveEntryApplication))]
	public abstract class BootstrapEntry<T> : BootstrapEntry
	{
		protected BootstrapEntry(WorldCollection collection) : base(collection)
		{
		}

		protected override void OnExecute(string jsonArgs)
		{
			OnExecute(JsonSerializer.Deserialize<T>(jsonArgs)!);
		}

		protected abstract void OnExecute(T args);
	}
}