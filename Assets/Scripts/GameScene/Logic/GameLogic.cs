using System;
using System.Linq;
using GameScene.Signals;
using Models;
using UniRx;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Logic
{
	public class GameLogic : IDisposable
	{
		private readonly LevelModel _levelModel;
		private readonly SignalBus _signalBus;

		private readonly CompositeDisposable _disposables;

		private readonly ReactiveCollection<RingColor> _tower1;
		private readonly ReactiveCollection<RingColor> _tower2;
		private readonly ReactiveCollection<RingColor> _tower3;

		private readonly ReactiveProperty<RingColor?> _hands;

		public IReadOnlyReactiveCollection<RingColor> Tower1 => _tower1;
		public IReadOnlyReactiveCollection<RingColor> Tower2 => _tower2;
		public IReadOnlyReactiveCollection<RingColor> Tower3 => _tower3;

		public IReadOnlyReactiveProperty<RingColor?> Hands => _hands;

		public int NumColors => _levelModel.NumColors;

		public GameLogic(LevelModel levelModel, SignalBus signalBus)
		{
			_levelModel = levelModel;
			_signalBus = signalBus;

			_tower1 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower1);
			_tower2 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower2);
			_tower3 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower3);

			_hands = new ReactiveProperty<RingColor?>(null);

			_disposables = new CompositeDisposable(_tower1, _tower2, _tower3, _hands);

			_signalBus.Subscribe<CatchRingSignal>(OnCatchRing);
			_signalBus.Subscribe<PlaceRingSignal>(OnPlaceRing);
		}

		private void OnCatchRing(CatchRingSignal signal)
		{
			Assert.IsTrue(!Hands.Value.HasValue || Hands.Value == signal.RingType, "Try to catch multiple rings.");

			if (!Hands.Value.HasValue)
			{
				ReactiveCollection<RingColor> fromTower;
				if (_tower1.LastOrDefault() == signal.RingType)
				{
					fromTower = _tower1;
				}
				else if (_tower2.LastOrDefault() == signal.RingType)
				{
					fromTower = _tower2;
				}
				else if (_tower3.LastOrDefault() == signal.RingType)
				{
					fromTower = _tower3;
				}
				else
				{
					throw new Exception("Can't find tower from which the ring was taken.");
				}

				fromTower.Remove(signal.RingType);
			}

			_hands.Value = signal.RingType;
		}

		private void OnPlaceRing(PlaceRingSignal signal)
		{
			Assert.IsTrue(Hands.Value == signal.RingType, "Try to place uncaught ring.");

			var toTower = signal.TowerIndex switch
			{
				0 => _tower1,
				1 => _tower2,
				2 => _tower3,
				_ => throw new IndexOutOfRangeException()
			};

			Assert.IsTrue(signal.PinIndex == toTower.Count, "Wrong ring index in the destination tower.");
			toTower.Add(signal.RingType);

			_hands.Value = null;
		}

		public bool CanTakeRing(RingColor ring) => Hands.Value == ring ||
		                                           (!Hands.Value.HasValue &&
		                                            (_tower1.Any() && _tower1.Last() == ring ||
		                                             _tower2.Any() && _tower2.Last() == ring ||
		                                             _tower3.Any() && _tower3.Last() == ring));

		void IDisposable.Dispose()
		{
			_signalBus.Unsubscribe<CatchRingSignal>(OnCatchRing);
			_signalBus.Unsubscribe<PlaceRingSignal>(OnPlaceRing);

			_disposables.Dispose();
		}
	}
}