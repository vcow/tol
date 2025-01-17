using Core.ScreenLocker;
using Core.SoundManager;
using Core.WindowManager;
using Models;
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
		}

		private void OnDestroy()
		{
			_signalBus.Unsubscribe<AddPlayerSignal>(OnAddPlayer);
			_signalBus.Unsubscribe<RemovePlayerSignal>(OnRemovePlayer);
		}

		private void OnAddPlayer(AddPlayerSignal signal)
		{
			_gameModelController.AddPlayer(signal.Name, signal.Scores, signal.LastLevel);
		}

		private void OnRemovePlayer(RemovePlayerSignal signal)
		{
			_gameModelController.RemovePlayer(signal.Name);
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