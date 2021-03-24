using STMasterServer.Shared.Services.Assets;

namespace StormiumTeam.GameBase.Network.MasterServer.AssetService
{
	public static class STAssetPointerExtensions
	{
		public static ResPath ToResPath(this STAssetPointer assetPointer, ResPath.EType type = ResPath.EType.MasterServer)
		{
			return new(type, assetPointer.Author, assetPointer.Mod, assetPointer.Id);
		}
	}
}