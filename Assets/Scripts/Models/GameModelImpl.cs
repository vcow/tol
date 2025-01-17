using System;
using UniRx;

namespace Models
{
	public sealed class GameModelImpl : IGameModel, IDisposable
	{
		private readonly CompositeDisposable _disposables;

		public readonly ReactiveCollection<IPlayerModel> Players;

		IReadOnlyReactiveCollection<IPlayerModel> IGameModel.Players => Players;

		public GameModelImpl()
		{
			Players = new ReactiveCollection<IPlayerModel>();
			_disposables = new CompositeDisposable(Players);
		}

		void IDisposable.Dispose()
		{
			_disposables.Dispose();
		}
	}
}