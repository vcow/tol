using System;
using UniRx;
using UnityEngine;

namespace Models
{
	public sealed class PlayerModelController : IDisposable
	{
		private readonly CompositeDisposable _disposables;
		private readonly PlayerModelImpl _playerModel;

		public IPlayerModel Model => _playerModel;

		public PlayerModelController(string name, uint scores, int lastLevel)
		{
			_playerModel = new PlayerModelImpl(name, scores, lastLevel);
			_disposables = new CompositeDisposable(_playerModel);
		}

		public PlayerModelController(PlayerModelRecord record) : this(record.name, record.scores, record.lastLevel)
		{
		}

		public uint AddScores(uint value, bool increment = true)
		{
			_playerModel.Scores.Value = increment ? _playerModel.Scores.Value + value : value;
			return _playerModel.Scores.Value;
		}

		public void SetLastLevel(int lastLevel)
		{
			_playerModel.LastLevel.Value = Mathf.Max(lastLevel, -1);
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