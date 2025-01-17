using Core.ScreenLocker;
using Core.SoundManager;
using Core.WindowManager;
using Models;
using Settings;
using StartScene.Signals;
using UI.Windows;
using UnityEngine;
using Zenject;

namespace StartScene
{
	public sealed class StartSceneController : MonoBehaviour
	{
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly IWindowManager _windowManager;
		[Inject] private readonly ISoundManager _soundManager;
		[Inject] private readonly SignalBus _signalBus;
		[Inject] private readonly GameModelController _gameModelController;
		[Inject] private readonly LevelsProvider _levelsProvider;
		[Inject] private readonly ZenjectSceneLoader _sceneLoader;

		private void Start()
		{
			if (_screenLockerManager.IsLocked)
			{
				_screenLockerManager.Unlock(OnSceneUnlock);
			}
			else
			{
				OnSceneUnlock(LockerType.Undefined);
			}

			_signalBus.Subscribe<AddPlayerSignal>(OnAddPlayer);
			_signalBus.Subscribe<RemovePlayerSignal>(OnRemovePlayer);
			_signalBus.Subscribe<StartPlayGameSignal>(OnStartPlayGame);
		}

		private void OnDestroy()
		{
			_signalBus.Unsubscribe<AddPlayerSignal>(OnAddPlayer);
			_signalBus.Unsubscribe<RemovePlayerSignal>(OnRemovePlayer);
			_signalBus.Unsubscribe<StartPlayGameSignal>(OnStartPlayGame);
		}

		private void OnAddPlayer(AddPlayerSignal signal)
		{
			_gameModelController.AddPlayer(signal.Name, signal.Scores, signal.LastLevel);
		}

		private void OnRemovePlayer(RemovePlayerSignal signal)
		{
			_gameModelController.RemovePlayer(signal.Name);
		}

		private void OnStartPlayGame(StartPlayGameSignal signal)
		{
			if (!_levelsProvider.Levels.TryGetValue(signal.FromLevel, out var levelModel))
			{
				Debug.LogError($"Can't find level {signal.FromLevel} in the LevelsProvider.");
				return;
			}

			var playerModelController = _gameModelController.GetPlayerController(signal.PlayerName);
			if (playerModelController == null)
			{
				Debug.LogError($"Can't find player {signal.PlayerName} in the PlayersList.");
				return;
			}

			_screenLockerManager.Lock(LockerType.SceneLoader, () =>
			{
				_sceneLoader.LoadSceneAsync(Const.GameSceneName, extraBindings: container =>
				{
					container.Bind<PlayerModelController>().FromInstance(playerModelController).AsSingle();
					container.Bind<LevelModel>().FromInstance(levelModel).AsSingle();
				});
			});
		}

		private void OnSceneUnlock(LockerType _)
		{
			_soundManager.PlayMusic("Lovers");
		}

		public void OnNewTest()
		{
			_windowManager.ShowWindow(NewPlayerWindow.Id);
		}

		public void OnReplay()
		{
			_windowManager.ShowWindow(PlayersListWindow.Id);
		}

		public void OnRating()
		{
			_windowManager.ShowWindow(RatingWindow.Id);
		}

		public void OnSettings()
		{
			_windowManager.ShowWindow(SettingsWindow.Id);
		}
	}
}