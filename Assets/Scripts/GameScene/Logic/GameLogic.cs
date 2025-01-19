using System;
using System.Linq;
using Models;
using UniRx;

namespace GameScene.Logic
{
	public class GameLogic : IDisposable
	{
		private readonly LevelModel _levelModel;

		private readonly CompositeDisposable _disposables;

		private readonly ReactiveCollection<RingColor> _tower1;
		private readonly ReactiveCollection<RingColor> _tower2;
		private readonly ReactiveCollection<RingColor> _tower3;

		private readonly ReactiveProperty<RingColor?> _hands;

		public IReadOnlyReactiveCollection<RingColor> Tower1 => _tower1;
		public IReadOnlyReactiveCollection<RingColor> Tower2 => _tower2;
		public IReadOnlyReactiveCollection<RingColor> Tower3 => _tower3;

		public IReadOnlyReactiveProperty<RingColor?> Hands => _hands;

		public GameLogic(LevelModel levelModel)
		{
			_levelModel = levelModel;

			_tower1 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower1);
			_tower2 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower2);
			_tower3 = new ReactiveCollection<RingColor>(_levelModel.InitialState.tower3);

			_hands = new ReactiveProperty<RingColor?>(null);

			_disposables = new CompositeDisposable(_tower1, _tower2, _tower3, _hands);
		}

		public bool CanTakeRing(RingColor ring) => Hands.Value == ring ||
		                                           (!Hands.Value.HasValue &&
		                                            (_tower1.Any() && _tower1.Last() == ring ||
		                                             _tower2.Any() && _tower2.Last() == ring ||
		                                             _tower3.Any() && _tower3.Last() == ring));

		void IDisposable.Dispose()
		{
			_disposables.Dispose();
		}
	}
}