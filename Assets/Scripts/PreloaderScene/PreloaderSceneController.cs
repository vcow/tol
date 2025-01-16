using Core.Assignments;
using Core.ScreenLocker;
using PreloaderScene.Assignments;
using UnityEngine;
using Zenject;

namespace PreloaderScene
{
	public sealed class PreloaderSceneController : MonoBehaviour
	{
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly ZenjectSceneLoader _sceneLoader;
		[Inject] private readonly RestoreGameStateAssignment _restoreGameStateAssignment;

		private void Start()
		{
			_screenLockerManager.Lock(LockerType.GameLoader, DoInitialize);
		}

		private void DoInitialize()
		{
			var initializeQueue = new AssignmentQueue();

			initializeQueue.Add(_restoreGameStateAssignment);
			// TODO: Add initialized objects here.

			initializeQueue.CompleteEvent += OnInitializeComplete;

			initializeQueue.Start();
		}

		private async void OnInitializeComplete(IAssignment assignment)
		{
			assignment.CompleteEvent -= OnInitializeComplete;
			await _sceneLoader.LoadSceneAsync(Const.StartSceneName);
		}
	}
}