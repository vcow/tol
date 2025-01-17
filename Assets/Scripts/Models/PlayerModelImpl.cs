using System;
using UniRx;

namespace Models
{
	public sealed class PlayerModelImpl : IPlayerModel, IDisposable
	{
		private readonly CompositeDisposable _disposables;

		public readonly ReactiveProperty<uint> Scores;
		public readonly IntReactiveProperty LastLevel;

		public string Name { get; }

		IReadOnlyReactiveProperty<uint> IPlayerModel.Scores => Scores;

		IReadOnlyReactiveProperty<int> IPlayerModel.LastLevel => LastLevel;

		public PlayerModelImpl(string name, uint scores = 0u, int lastLevel = -1)
		{
			Name = name;
			Scores = new ReactiveProperty<uint>(scores);
			LastLevel = new IntReactiveProperty(lastLevel);
			_disposables = new CompositeDisposable(Scores, LastLevel);
		}

		void IDisposable.Dispose()
		{
			_disposables.Dispose();
		}
	}
}