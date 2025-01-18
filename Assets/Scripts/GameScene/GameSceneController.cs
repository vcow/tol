using Core.ScreenLocker;
using GameScene.Controllers;
using Models;
using Settings;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene
{
	[DisallowMultipleComponent]
	public sealed class GameSceneController : MonoBehaviour
	{
		[Inject] private readonly IScreenLockerManager _screenLockerManager;

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
	}
}