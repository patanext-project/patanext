using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Core.IO;
using GameHost.Core.Threading;
using GameHost.IO;
using GameHost.Utility;
using Newtonsoft.Json;
using StormiumTeam.GameBase;

namespace PataNext.Game.GameItems
{
	public partial class GameItemsManager : AppSystem
	{
		private Dictionary<ResPath, Entity> idToEntityMap;

		private SameThreadTaskScheduler taskScheduler = new();

		public GameItemsManager(WorldCollection collection) : base(collection)
		{
			idToEntityMap = new();

			var helmEnt = collection.Mgr.CreateEntity();
			helmEnt.Set(new GameItemDescription("helm", "helm_desc"));
			helmEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Health = 60,
					Weight = 2
				}
			});
			
			Register(helmEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/helm/default_helm"));
			
			var swordEnt = collection.Mgr.CreateEntity();
			swordEnt.Set(new GameItemDescription("sword", "sword_desc"));
			swordEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Attack = 12,
					Weight = 3
				}
			});
			
			Register(swordEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/sword/default_sword"));
			
			var shieldEnt = collection.Mgr.CreateEntity();
			shieldEnt.Set(new GameItemDescription("shield", "shield_desc"));
			shieldEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Health = 25,
					Defense = 1,
					Weight = 1.5f
				}
			});
			
			Register(shieldEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/shield/default_shield"));
			
			
			var spearEnt = collection.Mgr.CreateEntity();
			spearEnt.Set(new GameItemDescription("sword", "sword_desc"));
			spearEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Attack = 8,
					Weight = 3
				}
			});
			
			Register(spearEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/spear/default_spear"));
			
			var bowEnt = collection.Mgr.CreateEntity();
			bowEnt.Set(new GameItemDescription("sword", "sword_desc"));
			bowEnt.Set(new EquipmentItemDescription
			{
				Additive = new()
				{
					Attack = 5,
					Weight = 1,
				}
			});
			
			Register(bowEnt, new(ResPath.EType.MasterServer, "st", "pn", "equipment/bow/default_bow"));
		}

		public Task<Entity[]> RegisterEquipmentsAsync(EquipmentItemMetadataStorage storage)
		{
			if (storage is null)
				throw new NullReferenceException(nameof(storage));
			
			async Task<Entity[]> @do()
			{
				var files = await storage.GetFilesAsync("*.json");
				var tasks = files.Select(async file =>
				{
					// This code should be close to the way it operate on MasterServer
					
					// Since the file may be in a collection we would need to get the source storage to get the base directory
					IStorage currStorage = storage;
					if (file is StorageCollectionFileSource fileSource)
					{
						currStorage = fileSource.Source;
						file        = fileSource.File;
					}
					
					var author = string.Empty;
					var mod    = string.Empty;

					var data = JsonConvert.DeserializeObject<Equipment>(Encoding.UTF8.GetString(await file.GetContentAsync()));
					if (string.IsNullOrEmpty(data.Id))
						data.Id = file.Name;
					
					if (string.IsNullOrEmpty(data.ItemType))
					{
						data.ItemType = Path.GetDirectoryName(file.FullName)!
						                    .Replace('\\', '/')
						                    .Replace(currStorage.CurrentPath!.Replace('\\', '/'), string.Empty)
						                    .Replace("_-", "/")
						                    .Replace("__", "/"); // Dll files seems to swap - to _ ?
						if (data.ItemType.StartsWith("/"))
							data.ItemType = data.ItemType[1..];

						if (file is DllEmbeddedFile) // dll files will replace . by /
																	 // this is not a problem most of the time but we need to constraint author.mod format
						{
							var chars = data.ItemType.ToCharArray();
							for (var i = 0; i < chars.Length; i++)
							{
								if (chars[i] == '/')
								{
									chars[i] = '.';
									break;
								}
							}

							data.ItemType = new(chars);
						}
					}

					author = data.ItemType[..data.ItemType.IndexOf('.')];
					mod = data.ItemType[(data.ItemType.IndexOf('.') + 1)..data.ItemType.IndexOf('/')];

					var            finalId = "equipment/" + data.ItemType[(data.ItemType.LastIndexOf('/') + 1)..] + "/" + data.Id;
					FinalEquipment final;
					final.ResPath     = new ResPath(ResPath.EType.MasterServer, author, mod, finalId);
					final.Name        = data.Name;
					final.Description = data.Description;
					final.IsDefault   = data.IsDefault;
					final.ItemType    = data.ItemType;
					final.File        = new(file);

					return final;
				}).ToArray();

				var entities = new Entity[tasks.Length];
				var i        = 0;
				foreach (var task in tasks)
				{
					var final  = await task;
					var entity = World.Mgr.CreateEntity();

					Console.WriteLine($"{final.ItemType} {final.ResPath}");

					await final.File.FillDescription(entity);

					Register(entity, final.ResPath);

					entities[i++] = entity;
				}

				return entities;
			}

			return taskScheduler.StartUnwrap(@do);
		}

		public void Register(Entity entity, ResPath resPath)
		{
			Debug.Assert(entity.Has<GameItemDescription>(), "entity.Has<GameItemDescription>()");

			idToEntityMap[resPath] = entity;
		}

		public bool TryGetDescription(ResPath resPath, out Entity entity)
		{
			return idToEntityMap.TryGetValue(resPath, out entity);
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			
			taskScheduler.Execute();
		}
	}
}