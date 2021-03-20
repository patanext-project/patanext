using PataNext.Game.Rpc.SerializationUtility;
using StormiumTeam.GameBase;

namespace PataNext.Game.Rpc
{
	public struct GetInventoryRpc
	{
		/// <summary>
		/// If true, it will only search for categories inside <see cref="FilterCategories"/>.
		/// If false, it will search for all except categories inside <see cref="FilterCategories"/>
		/// </summary>
		public bool      FilterInclude;
		public ResPath[] FilterCategories;
		
		public struct Response
		{
			public struct Item
			{
				public MasterServerItemId Id;
				public ResPath            AssetType;
				public MasterServerUnitId EquippedBy;
			}

			public Item[] Items;
		}
	}
}