using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Text;
using GameHost.Core.Ecs;
using GameHost.Native.Char;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.Utility.Resource;
using Microsoft.Extensions.Logging;
using PataNext.Module.Simulation.Resources;
using StormiumTeam.GameBase;
using ZLogger;

namespace PataNext.Module.Simulation.Systems
{
	public delegate void SetKitInformationDelegate(GameEntityHandle kitResourceHandle);

	public class KitCollectionSystem : AppSystem
	{
		private GameResourceDb<UnitKitResource>  kitDb;
		private GameResourceDb<UnitRoleResource> roleDb;

		private Dictionary<ResPath, HashSet<SetKitInformationDelegate>> delegateKitMap  = new();
		private Dictionary<ResPath, HashSet<SetKitInformationDelegate>> delegateRoleMap = new();

		private ILogger logger;

		public KitCollectionSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref kitDb);
			DependencyResolver.Add(() => ref roleDb);
			DependencyResolver.Add(() => ref logger);
		}

		public void RegisterKit(SetKitInformationDelegate setKitInformation, [NotNull] ResPath[] targets)
		{
			foreach (var target in targets)
			{
				if (!delegateKitMap.TryGetValue(target, out var hashSet))
					hashSet = delegateKitMap[target] = new();

				hashSet.Add(setKitInformation);
			}
		}

		public void RegisterRole(SetKitInformationDelegate setKitInformation, [NotNull] ResPath[] targets)
		{
			foreach (var target in targets)
			{
				if (!delegateRoleMap.TryGetValue(target, out var hashSet))
					hashSet = delegateRoleMap[target] = new();

				hashSet.Add(setKitInformation);
			}
		}

		public GameResource<UnitKitResource> GetKit(ResPath resPath)
		{
			using var builder = ZString.CreateStringBuilder();
			builder.Append(resPath.Author);
			builder.Append('.');
			builder.Append(resPath.ModPack);
			builder.Append('/');
			builder.Append(resPath.Resource);

			var resource = kitDb.GetOrCreate(new(CharBufferUtility.Create<CharBuffer64>(builder.AsArraySegment().AsSpan())));

			if (delegateKitMap.TryGetValue(resPath, out var hashSet))
			{
				foreach (var func in hashSet)
				{
					func(resource.Handle);
				}
			}
			else
				logger.ZLogWarning("No delegate set for {0}", resPath.FullString);

			return resource;
		}

		public GameResource<UnitRoleResource> GetRole(ResPath resPath)
		{
			using var builder = ZString.CreateStringBuilder();
			builder.Append(resPath.Author);
			builder.Append('.');
			builder.Append(resPath.ModPack);
			builder.Append('/');
			builder.Append(resPath.Resource);

			var resource = roleDb.GetOrCreate(new(CharBufferUtility.Create<CharBuffer64>(builder.AsArraySegment().AsSpan())));

			if (delegateRoleMap.TryGetValue(resPath, out var hashSet))
			{
				foreach (var func in hashSet)
				{
					func(resource.Handle);
				}
			}
			else
				logger.ZLogWarning("No delegate set for {0}", resPath.FullString);

			return resource;
		}
	}
}