using System.Collections.Generic;
using System.Threading.Tasks;
using GameHost.Core.IO;
using GameHost.IO;

namespace PataNext.Game.Abilities
{
	/// <summary>
	/// A storage containing Ability description data
	/// </summary>
	/// <remarks>
	/// The module in that assembly must have this in context
	/// </remarks>
	public class AbilityDescStorage : ChildStorage
	{
		public AbilityDescStorage(IStorage root, IStorage parent) : base(root, parent)
		{
		}

		public AbilityDescStorage(IStorage parent) : this(parent, parent)
		{
		}
	}
}