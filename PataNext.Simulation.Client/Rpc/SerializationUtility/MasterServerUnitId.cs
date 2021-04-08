namespace PataNext.Game.Rpc.SerializationUtility
{
	public struct MasterServerSaveId
	{
		public string Value { get; set; }

		public MasterServerSaveId(string value)
		{
			Value = value;
		}
	}
	
	public struct MasterServerUnitId
	{
		public string Value { get; set; }

		public MasterServerUnitId(string value)
		{
			Value = value;
		}
	}
	
	public struct MasterServerUnitPresetId
	{
		public string Value { get; set; }

		public MasterServerUnitPresetId(string value)
		{
			Value = value;
		}
	}
	
	public struct MasterServerItemId
	{
		public string Value { get; set; }

		public MasterServerItemId(string value)
		{
			Value = value;
		}
	}
}