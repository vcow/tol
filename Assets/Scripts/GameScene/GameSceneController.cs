using Core.ScreenLocker;
using UnityEngine;
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