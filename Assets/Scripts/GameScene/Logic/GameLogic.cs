using System;
using System.Collections.Generic;
using System.Linq;
using GameScene.Signals;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Logic
{
	public class GameLogic : IDisposable
	{
		public enum Result
		{
			Undefined,
			Win,
			Lose
		}

		private const uint MaxScores = 100;

		private readonly LevelModel _levelModel;
		private readonly PlayerModelController _playerModelController;
		private readonly SignalBus _signalBus;

		private readonly CompositeDisposable _disposables;

		private readonly ReactiveCollection<RingColor> _tower1;
		private readonly ReactiveCollection<RingColor> _tower2;
		private readonly ReactiveCollection<RingColor> _tower3;

		private readonly ReactiveProperty<RingColor?> _hands;
		private readonly ReactiveProperty<Result> _gameResult;
		private readonly IntReactiveProperty _step;

		private int _fromTowerIndex;

		public IReadOnlyReactiveCollection<RingColor> Tower1 => _tower1;
		public IReadOnlyReactiveCollection<RingColor> Tower2 => _tower2;
		public IReadOnlyReactiveCollection<RingColor> Tower3 => _tower3;

		public IReadOnlyReactiveProperty<RingColor?> Hands => _hands;

		public int NumColors => _levelModel.NumColors;

		public int MaxSteps => _levelModel.MaxStepsNum;

		public IReadOnlyReactiveProperty<int> Step => _step;

		public IReadOnlyReactiveProperty<uint> Scores { get; }

		public IReadOnlyReactiveProperty<Result> GameResult => _gameResult;

		public bool GameIsOver => GameResult.Value != Result.Undefined;

		public GameLogic(LevelModel levelModel, PlayerModelController playerModelController, SignalBus signalBus)
		{
			_levelModel = levelModel;
			_playerModelController = playerModelController;
			_signalBus = signalBus;

			_tower1 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower1);
			_tower2 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower2);
			_tower3 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower3);

			_hands = new ReactiveProperty<RingColor?>(null);
			_step = new IntReactiveProperty(0);
			_gameResult = new ReactiveProperty<Result>(Result.Undefined);

			var scores = _step.Select(CalcScoresFromStep).ToReadOnlyReactiveProperty();
			Scores = scores;

			_disposables = new CompositeDisposable(_tower1, _tower2, _tower3, _hands,
				_step, _gameResult, scores);

			_signalBus.Subscribe<CatchRingSignal>(OnCatchRing);
			_signalBus.Subscribe<PlaceRingSignal>(OnPlaceRing);
		}

		public bool CanTakeRing(RingColor ring) => Hands.Value == ring ||
		                                           (!Hands.Value.HasValue &&
		                                            (_tower1.Any() && _tower1.Last() == ring ||
		                                             _tower2.Any() && _tower2.Last() == ring ||
		                                             _tower3.Any() && _tower3.Last() == ring));

		private uint CalcScoresFromStep(int step)
		{
			if (step <= _levelModel.MinStepsNum)
			{
				return MaxScores;
			}

			var numSteps = (uint)Mathf.Max(_levelModel.MaxStepsNum - _levelModel.MinStepsNum, 0) + 1;
			var delta = MaxScores / numSteps;

			return MaxScores - (uint)(step - _levelModel.MinStepsNum) * delta;
		}

		private void OnCatchRing(CatchRingSignal signal)
		{
			if (GameIsOver)
			{
				throw new Exception("Game is over.");
			}

			Assert.IsTrue(!Hands.Value.HasValue || Hands.Value == signal.RingType, "Try to catch multiple rings.");

			if (!Hands.Value.HasValue)
			{
				ReactiveCollection<RingColor> fromTower;
				if (_tower1.LastOrDefault() == signal.RingType)
				{
					fromTower = _tower1;
					_fromTowerIndex = 0;
				}
				else if (_tower2.LastOrDefault() == signal.RingType)
				{
					fromTower = _tower2;
					_fromTowerIndex = 1;
				}
				else if (_tower3.LastOrDefault() == signal.RingType)
				{
					fromTower = _tower3;
					_fromTowerIndex = 2;
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

			if (signal.TowerIndex == _fromTowerIndex)
			{
				// Placed back.
				return;
			}

			_step.Value += 1;

			var (goalTower1, goalTower2, goalTower3) = _levelModel.GoalState;
			if (MatchTowers(Tower1, goalTower1) && MatchTowers(Tower2, goalTower2) && MatchTowers(Tower3, goalTower3))
			{
				// win
				_playerModelController.AddScores(Scores.Value);
				_playerModelController.SetLastLevel(_levelModel.Index);
				_gameResult.Value = Result.Win;
			}
			else if (Step.Value >= MaxSteps)
			{
				// lose
				_gameResult.Value = Result.Lose;
			}
		}

		private bool MatchTowers(IEnumerable<RingColor> tower1, IEnumerable<RingColor> tower2)
		{
			using var tower1Enumerator = tower1.GetEnumerator();
			using var tower2Enumerator = tower2.GetEnumerator();
			while (tower1Enumerator.MoveNext())
			{
				if (!tower2Enumerator.MoveNext() || tower1Enumerator.Current != tower2Enumerator.Current)
				{
					return false;
				}
			}

			if (tower2Enumerator.MoveNext())
			{
				return false;
			}

			return true;
		}

		void IDisposable.Dispose()
		{
			_signalBus.Unsubscribe<CatchRingSignal>(OnCatchRing);
			_signalBus.Unsubscribe<PlaceRingSignal>(OnPlaceRing);

			_disposables.Dispose();
		}
	}
}