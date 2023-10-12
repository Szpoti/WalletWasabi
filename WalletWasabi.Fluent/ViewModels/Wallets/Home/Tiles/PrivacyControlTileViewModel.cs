using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using NBitcoin;
using ReactiveUI;
using WalletWasabi.Fluent.Extensions;
using WalletWasabi.Fluent.Models.Wallets;
using WalletWasabi.Fluent.ViewModels.Wallets.Home.Tiles.PrivacyRing;
using WalletWasabi.Logging;

namespace WalletWasabi.Fluent.ViewModels.Wallets.Home.Tiles;

public partial class PrivacyControlTileViewModel : ActivatableViewModel, IPrivacyRingPreviewItem
{
	private readonly IWalletModel _wallet;
	[AutoNotify] private bool _fullyMixed;
	[AutoNotify] private string _percentText = "";
	[AutoNotify] private Money _balancePrivate = Money.Zero;
	[AutoNotify] private bool _hasPrivateBalance;
	[AutoNotify] private bool _showPrivacyBar;

	private PrivacyControlTileViewModel(IWalletModel wallet, bool showPrivacyBar = true)
	{
		_wallet = wallet;
		_showPrivacyBar = showPrivacyBar;

		var canShowDetails = _wallet.HasBalance;

		ShowDetailsCommand = ReactiveCommand.Create(ShowDetails, canShowDetails);

		if (showPrivacyBar)
		{
			PrivacyBar = new PrivacyBarViewModel(wallet);
		}
	}

	public ICommand ShowDetailsCommand { get; }

	public PrivacyBarViewModel? PrivacyBar { get; }

	protected override void OnActivated(CompositeDisposable disposables)
	{
		base.OnActivated(disposables);

		var b = BenchmarkLogger.Measure();
		var coins = _wallet.Coins.List.ToCollection();
		b.Dispose();

		b = BenchmarkLogger.Measure();
		var x = _wallet.Privacy.Progress
			.CombineLatest(_wallet.Privacy.IsWalletPrivate)
			.CombineLatest(coins)
			.Flatten();
		b.Dispose();

		b = BenchmarkLogger.Measure();
		x.Do(tuple =>
		{
			var (privacyProgress, isWalletPrivate, coins) = tuple;
			Update(privacyProgress, isWalletPrivate, coins);
		}).Subscribe().DisposeWith(disposables);
		b.Dispose();

		b = BenchmarkLogger.Measure();
		PrivacyBar?.Activate(disposables);
		b.Dispose();
	}

	private void ShowDetails()
	{
		UiContext.Navigate().To().PrivacyRing(_wallet);
	}

	private void Update(int privacyProgress, bool isWalletPrivate, IReadOnlyCollection<ICoinModel> coins)
	{
		PercentText = $"{privacyProgress} %";
		FullyMixed = isWalletPrivate;

		BalancePrivate = coins.Where(x => x.IsPrivate).TotalAmount();
		HasPrivateBalance = BalancePrivate > Money.Zero;
	}
}
