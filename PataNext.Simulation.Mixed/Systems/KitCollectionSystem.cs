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
		private GameResourceDb<UnitKitResource>                         kitDb;
		private Dictionary<ResPath, HashSet<SetKitInformationDelegate>> delegateMap = new();

		private ILogger logger;

		public KitCollectionSystem(WorldCollection collection) : base(collection)
		{
			DependencyResolver.Add(() => ref kitDb);
			DependencyResolver.Add(() => ref logger);
		}

		public void Register(SetKitInformationDelegate setKitInformation, [NotNull] ResPath[] targets)
		{
			foreach (var target in targets)
			{
				if (!delegateMap.TryGetValue(target, out var hashSet))
					hashSet = delegateMap[target] = new();

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

			if (delegateMap.TryGetValue(resPath, out var hashSet))
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