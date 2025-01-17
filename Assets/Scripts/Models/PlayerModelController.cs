using System;
using UniRx;

namespace Models
{
	public sealed class PlayerModelController : IDisposable
	{
		private readonly CompositeDisposable _disposables;
		private readonly PlayerModelImpl _playerModel;

		public IPlayerModel Model => _playerModel;

		public PlayerModelController(string name, uint scores, int lastLevel)
		{
			_playerModel = new PlayerModelImpl(name);
			_disposables = new CompositeDisposable(_playerModel);
		}

		public PlayerModelController(PlayerModelRecord record) : this(record.name, record.scores, record.lastLevel)
		{
		}

		public static implicit operator PlayerModelRecord(PlayerModelController controller) => new PlayerModelRecord
		{
			name = controller.Model.Name,
			scores = controller.Model.Scores.Value,
			lastLevel = controller.Model.LastLevel.Value
		};

		void IDisposable.Dispose()
		{
			_disposables.Dispose();
		}
	}
}