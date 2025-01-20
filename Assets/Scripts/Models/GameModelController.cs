using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;

namespace Models
{
	public class GameModelController : IDisposable
	{
		private readonly Dictionary<string, PlayerModelController> _playerModelControllers = new();

		private readonly Subject<Unit> _observeModelChanged;
		private readonly CompositeDisposable _disposables;
		private readonly GameModelImpl _gameModel;

		public IGameModel Model => _gameModel;

		public IObservable<Unit> ObserveModelChanged => _observeModelChanged;

		public GameModelController(string rawData = null)
		{
			_gameModel = new GameModelImpl();
			_observeModelChanged = new Subject<Unit>();
			_disposables = new CompositeDisposable(_gameModel, _observeModelChanged);

			if (!string.IsNullOrEmpty(rawData))
			{
				RestoreGameModel(rawData);
			}

			_gameModel.Players.ObserveAdd()
				.Subscribe(_ => _observeModelChanged.OnNext(Unit.Default))
				.AddTo(_disposables);
			_gameModel.Players.ObserveRemove()
				.Subscribe(_ => _observeModelChanged.OnNext(Unit.Default))
				.AddTo(_disposables);
		}

		public void RestoreGameModel(string rawData)
		{
			Assert.IsFalse(string.IsNullOrEmpty(rawData), "Restored data is empty.");

			var gameRecord = JsonConvert.DeserializeObject<GameModelRecord>(rawData);
			foreach (var pair in gameRecord.players)
			{
				var value = pair.Value;
				value.name = pair.Key;
				var playerModelController = new PlayerModelController(value);
				AddPlayerModelController(pair.Key, playerModelController);
			}
		}

		public PlayerModelController AddPlayer(string name, uint scores = 0u, int lastLevel = -1)
		{
			if (_playerModelControllers.TryGetValue(name, out var playerModelController))
			{
				Debug.LogError($"Player with the name {name} already added.");
			}
			else
			{
				playerModelController = new PlayerModelController(name, scores, lastLevel);
				AddPlayerModelController(name, playerModelController);
			}

			return playerModelController;
		}

		private void AddPlayerModelController(string name, PlayerModelController playerModelController)
		{
			_playerModelControllers.Add(name, playerModelController);
			_disposables.Add(playerModelController);
			_gameModel.Players.Add(playerModelController.Model);

			new[]
				{
					playerModelController.Model.Scores.Skip(1).Select(_ => Unit.Default),
					playerModelController.Model.LastLevel.Skip(1).Select(_ => Unit.Default)
				}
				.Merge()
				.Subscribe(unit => _observeModelChanged.OnNext(unit))
				.AddTo(_disposables);
		}

		public bool RemovePlayer(string name)
		{
			if (!_playerModelControllers.TryGetValue(name, out var playerModelController))
			{
				return false;
			}

			_gameModel.Players.Remove(playerModelController.Model);
			_disposables.Remove(playerModelController);
			_playerModelControllers.Remove(name);
			return true;
		}

		public IPlayerModel ResetPlayer(string name)
		{
			if (!_playerModelControllers.TryGetValue(name, out var playerModelController))
			{
				return null;
			}

			playerModelController.SetLastLevel(-1);
			playerModelController.AddScores(0, false);
			return playerModelController.Model;
		}

		public PlayerModelController GetPlayerController(string name)
		{
			return _playerModelControllers.GetValueOrDefault(name);
		}

		public override string ToString()
		{
			var players = _playerModelControllers.Values
				.Select(controller => (PlayerModelRecord)controller).ToDictionary(rec => rec.name);
			var record = new GameModelRecord
			{
				players = players
			};
			return JsonConvert.SerializeObject(record);
		}

		void IDisposable.Dispose()
		{
			_observeModelChanged.OnCompleted();
			_disposables.Dispose();
			_playerModelControllers.Clear();
		}
	}
}