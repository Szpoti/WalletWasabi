using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletWasabi.WabiSabi.Backend.Rounds.CoinJoinStorage;

public interface ICoinJoinIdStore
{
	public void Append(uint256 id);

	public bool Contains(uint256 id);
}
