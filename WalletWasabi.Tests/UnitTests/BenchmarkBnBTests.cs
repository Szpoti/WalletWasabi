using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NBitcoin;
using System.Collections.Generic;
using WalletWasabi.BranchNBound;
using Xunit;

namespace WalletWasabi.Tests.UnitTests
{
	public class BenchmarkBnBTests
	{
		private Random Random { get; } = new();
		private List<Money> Coins100 { get; } = new();
		private List<Money> Coins1000 { get; } = new();
		private List<Money> Coins4000 { get; } = new();
		private List<Money> Coins10000 { get; } = new();

		public BenchmarkBnBTests()
		{
			FillLists();
		}

		private void FillLists()
		{
			ulong money;

			for (int i = 0; i < 100; i++)
			{
				money = (ulong)Random.Next(2500, 10001);
				Coins100.Add(money);
			}

			for (int i = 0; i < 1000; i++)
			{
				money = (ulong)Random.Next(2500, 100000001);
				Coins1000.Add(money);
			}

			for (int i = 0; i < 4000; i++)
			{
				money = (ulong)Random.Next(2500, 100000001);
				Coins4000.Add(money);
			}

			for (int i = 0; i < 10000; i++)
			{
				money = (ulong)Random.Next(2500, 100000001);
				Coins10000.Add(money);
			}
		}

		// Use these for benchmarking

		[Benchmark]
		public List<Money> BnBIter4Simple()
		{
			var utxos = new List<Money> { Money.Satoshis(12), Money.Satoshis(10), Money.Satoshis(10), Money.Satoshis(5), Money.Satoshis(4) };

			Money target = Money.Satoshis(19);
			var selector = new BranchAndBoundIter4(utxos);

			_ = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
			return selectedCoins;
		}

		[Benchmark]
		public List<Money> BnBIter4List100()
		{
			Money target = Money.Satoshis(100000000);
			var selector = new BranchAndBoundIter4(Coins100);

			_ = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
			return selectedCoins;
		}

		[Benchmark]
		public List<Money> BnBIter4List1000()
		{
			Money target = Money.Satoshis(100000000);
			var selector = new BranchAndBoundIter4(Coins1000);

			_ = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
			return selectedCoins;
		}

		[Benchmark]
		public List<Money> BnBIter4List4000()
		{
			Money target = Money.Satoshis(100000000);
			var selector = new BranchAndBoundIter4(Coins4000);

			_ = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
			return selectedCoins;
		}

		[Benchmark]
		public List<Money> BnBIter4List10000()
		{
			Money target = Money.Satoshis(100000000);
			var selector = new BranchAndBoundIter4(Coins10000);

			_ = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
			return selectedCoins;
		}

		// Use these for testing via VisualStudio

		//	[Fact]
		//	public void BnBIter4Simple()
		//	{
		//		var utxos = new List<Money> { Money.Satoshis(12), Money.Satoshis(10), Money.Satoshis(10), Money.Satoshis(5), Money.Satoshis(4) };

		//		Money target = Money.Satoshis(19);
		//		var selector = new BranchAndBoundIter4(utxos);

		//		bool wasSuccess = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
		//		Assert.True(wasSuccess);
		//	}

		//	[Fact]
		//	public void BnBIter4List100()
		//	{
		//		Money target = Money.Satoshis(10000);
		//		var selector = new BranchAndBoundIter4(Coins100);

		//		bool wasSuccess = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
		//		Assert.True(wasSuccess);
		//	}

		//	[Fact]
		//	public void BnBIter4List1000()
		//	{
		//		Money target = Money.Satoshis(100000000);
		//		var selector = new BranchAndBoundIter4(Coins1000);

		//		bool wasSuccess = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
		//		Assert.True(wasSuccess);
		//	}

		//	[Fact]
		//	public void BnBIter4List4000()
		//	{
		//		Money target = Money.Satoshis(100000000);
		//		var selector = new BranchAndBoundIter4(Coins4000);

		//		bool wasSuccess = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
		//		Assert.True(wasSuccess);
		//	}

		//	[Fact]
		//	public void BnBIter4List10000()
		//	{
		//		Money target = Money.Satoshis(100000000);
		//		var selector = new BranchAndBoundIter4(Coins10000);

		//		bool wasSuccess = selector.TryGetExactMatch(target, out List<Money>? selectedCoins);
		//		Assert.True(wasSuccess);
		//	}
	}

	public class Program
	{
		public static void Main()
		{
			_ = BenchmarkRunner.Run<BenchmarkBnBTests>();
		}
	}
}
