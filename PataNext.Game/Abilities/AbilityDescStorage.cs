using System.Collections.Generic;
using System.Threading.Tasks;
using GameHost.Core.IO;

namespace PataNext.Game.Abilities
{
	public interface IModuleHasAbilityDescStorage
	{
		AbilityDescStorage Value { get; }
	}
	
	/// <summary>
	/// A storage containing Ability description data
	/// </summary>
	/// <remarks>
	/// The module in that assembly must implement IModuleHasAbilityDescStorage
	/// </remarks>
	public class AbilityDescStorage : IStorage
	{
		public readonly IStorage parent;

		public AbilityDescStorage(IStorage parent)
		{
			this.parent = parent;
		}

		public string CurrentPath => parent.CurrentPath;

		public Task<IEnumerable<IFile>> GetFilesAsync(string             pattern) => parent.GetFilesAsync(pattern);
		public Task<IStorage>           GetOrCreateDirectoryAsync(string path)    => parent.GetOrCreateDirectoryAsync(path);
	}
}