using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DefaultEcs;
using GameHost.Core.IO;
using StormiumTeam.GameBase;

namespace PataNext.Game.GameItems
{
	public abstract class BaseGameItemMetadataStorage : IStorage
	{
		public readonly IStorage Parent;

		public BaseGameItemMetadataStorage(IStorage parent)
		{
			Parent = parent;
		}

		public string CurrentPath => Parent.CurrentPath;

		public Task<IEnumerable<IFile>> GetFilesAsync(string pattern)
		{
			return Parent.GetFilesAsync(pattern);
		}

		public Task<IStorage> GetOrCreateDirectoryAsync(string path)
		{
			return Parent.GetOrCreateDirectoryAsync(path);
		}
	}

	public class GameItemMetadataFile : IFile
	{
		public readonly IFile Base;

		public GameItemMetadataFile(IFile file)
		{
			Base = file;
		}

		public string Name     => Base.Name;
		public string FullName => Base.FullName;

		public Task<byte[]> GetContentAsync()
		{
			return Base.GetContentAsync();
		}

		// since it's much more complex than a BgmDescription and that there can be child classes, we must fill the info on an entity.
		// we could have actually returned a IItemDescription but it would allocate more data, which we don't want to
		public virtual async Task FillDescription(Entity target)
		{
			var jsonBytes = await GetContentAsync();
			var document  = JsonDocument.Parse(jsonBytes);

			target.Set(new GameItemDescription(
				default,
				default,
				document.RootElement.GetProperty("name").GetString(),
				document.RootElement.GetProperty("description").GetString()
			));
			if (document.RootElement.TryGetProperty("stackable", out var stackableProp)
			    && stackableProp.GetBoolean())
				target.Set(new GameItemIsStackable());

			FillDescriptionChildClass(target, document);
		}

		protected virtual void FillDescriptionChildClass(Entity target, JsonDocument document)
		{
		}
	}

	public struct GameItemDescription
	{
		public ResPath Id;
		public string  Type;

		public string Name;
		public string Description;

		public GameItemDescription(ResPath id, string type, string name, string description)
		{
			Id   = id;
			Type = type;
			
			Name        = name;
			Description = description;
		}
	}

	public struct GameItemIsStackable
	{

	}
}