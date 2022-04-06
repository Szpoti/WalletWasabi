using NBitcoin;

namespace WalletWasabi.WabiSabi.Backend.Rounds.CoinJoinStorage;

public interface ICoinJoinIdStore
{
	public void Append(uint256 id);

	public bool Contains(uint256 id);

	public void FetchOldCoinJoins(string coinJoinsFilePath, string coinJoinIdStoreFilePath);
}
