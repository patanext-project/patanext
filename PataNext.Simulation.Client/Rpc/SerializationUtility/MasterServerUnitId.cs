namespace PataNext.Game.Rpc.SerializationUtility
{
	public struct MasterServerUnitId
	{
		public string Value { get; set; }

		public MasterServerUnitId(string value)
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