using System.Collections.Generic;
using Newtonsoft.Json;
using StormiumTeam.GameBase;

namespace PataNext.Game.GameItems
{
	public partial class GameItemsManager
	{
		// Those classes are copied from the MasterServer code
		// They're used for deserializing item and equipment data

		public class AssetBase
		{
			[JsonProperty("id")]
			public string Id;

			[JsonProperty("name")]
			public string Name;

			[JsonProperty("description")]
			public string Description;
		}

		private class ItemBase : AssetBase
		{
			[JsonProperty("itemType")]
			public string ItemType;
		}

		private class Equipment : ItemBase
		{
			[JsonProperty("default")]
			public bool IsDefault;

			[JsonProperty("stats")]
			public Dictionary<string, object> Statistics;
		}

		private struct FinalEquipment
		{
			public ResPath ResPath;
			public string  Name;
			public string  Description;
			public string  ItemType;
			public bool    IsDefault;

			public EquipmentItemMetadataFile File;
		}
	}
}