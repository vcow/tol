using Core.ScreenLocker;
using Core.WindowManager;
using UI.Windows;
using UnityEngine;
using Zenject;

namespace StartScene
{
	public sealed class StartSceneController : MonoBehaviour
	{
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly IWindowManager _windowManager;

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
		}

		private void OnSceneUnlock(LockerType _)
		{
		}

		public void OnNewTest()
		{
		}

		public void OnReplay()
		{
		}

		public void OnRating()
		{
		}

		public void OnSettings()
		{
			_windowManager.ShowWindow(SettingsWindow.Id);
		}
	}
}