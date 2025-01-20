using Core.ScreenLocker;
using Core.SoundManager;
using Core.WindowManager;
using UI.Windows;
using UnityEngine;
using Zenject;

namespace GameScene
{
	[DisallowMultipleComponent]
	public sealed class GameSceneController : MonoBehaviour
	{
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly IWindowManager _windowManager;
		[Inject] private readonly ISoundManager _soundManager;

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

			_soundManager.PlayMusic("Together");
		}

		private void OnSceneUnlock(LockerType _)
		{
		}

		public void OnOpenSettings()
		{
			_windowManager.ShowWindow(SettingsWindow.Id);
		}
	}
}